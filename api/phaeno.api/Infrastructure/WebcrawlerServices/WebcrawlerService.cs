using AngleSharp;
using Microsoft.Extensions.Options;
using phaeno.api.Configuration;
using phaeno.api.Infrastructure.WebcrawlerServices.SupportFiles;
using phaeno.api.Infrastructure.WebSearchServices;
using phaeno.api.Infrastructure.WebSearchServices.SupportingFiles;
using System.Collections.Concurrent;

namespace phaeno.api.Infrastructure.WebcrawlerServices;

public sealed class WebcrawlerService : IWebcrawlerService
{
    private readonly HttpClient httpClient;
    private readonly IWebSearchService searchService;
    private readonly ILogger<WebcrawlerService> logger;
    private readonly IBrowsingContext browsingContext;

    private readonly ConcurrentDictionary<string, byte> visitedPages = new();
    private readonly ConcurrentDictionary<string, byte> visitedSitemaps = new();

    private readonly RobotsTxtHelper robots;
    private readonly WebCrawlerSettings settings;

    public WebcrawlerService(
        HttpClient httpClient,
        IWebSearchService searchService,
        IOptions<WebCrawlerSettings> settings,
        ILogger<WebcrawlerService> logger)
    {
        this.httpClient = httpClient;
        this.searchService = searchService;
        this.logger = logger;
        this.settings = settings.Value;

        var config = AngleSharp.Configuration.Default.WithDefaultLoader();
        browsingContext = BrowsingContext.New(config);

        // Minimal robots helper; also parses Sitemap: lines
        robots = new RobotsTxtHelper(httpClient, userAgent: "phaeno-crawler");
    }

    public async Task CrawlPhaenoSiteAsync(CancellationToken ct = default)
    {
        var siteUrl = new Uri(settings.Url);
        var fallbackSitemapUrl = new Uri(siteUrl, settings.SiteMap);

        visitedPages.Clear();
        visitedSitemaps.Clear();

        var results = new List<IndexedPage>();

        await robots.LoadAsync(siteUrl, ct);

        // Prefer robots-advertised sitemap if present; otherwise use configured value
        var startSitemap = robots.Sitemaps.FirstOrDefault() ?? fallbackSitemapUrl;

        await CrawlFromSitemapAsync(siteUrl, startSitemap, results, ct);

        searchService.RebuildIndex(results);
    }

    private async Task CrawlFromSitemapAsync(
        Uri root,
        Uri sitemapUrl,
        List<IndexedPage> results,
        CancellationToken ct)
    {
        // robots matching is generally path-based
        if (!robots.IsAllowed(sitemapUrl.AbsolutePath))
            return;

        // Avoid recursion cycles / repeated sitemap fetches
        var normalizedSitemap = NormalizeAbsoluteUrlNoQueryNoFragment(sitemapUrl);
        if (!visitedSitemaps.TryAdd(normalizedSitemap, 0))
            return;

        string xml;
        try
        {
            xml = await httpClient.GetStringAsync(sitemapUrl, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load sitemap {Url}", sitemapUrl);
            return;
        }

        SitemapParser.SitemapParseResult parsed;
        try
        {
            parsed = SitemapParser.Parse(xml, root);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to parse sitemap {Url}", sitemapUrl);
            return;
        }

        // Sitemap index -> recurse
        if (parsed.ChildSitemaps.Count > 0)
        {
            foreach (var child in parsed.ChildSitemaps.Distinct())
            {
                ct.ThrowIfCancellationRequested();
                await CrawlFromSitemapAsync(root, child, results, ct);
            }
            return;
        }

        // Urlset -> crawl pages
        foreach (var url in parsed.Urls.Distinct())
        {
            ct.ThrowIfCancellationRequested();

            // Safety: only crawl within the configured root host
            if (!string.Equals(url.Host, root.Host, StringComparison.OrdinalIgnoreCase))
                continue;

            await CrawlPageAsync(root, url, results, ct);
        }
    }

    private async Task CrawlPageAsync(
        Uri root,
        Uri url,
        List<IndexedPage> results,
        CancellationToken ct)
    {
        // robots checks should be path-based
        if (!robots.IsAllowed(url.AbsolutePath))
            return;

        var normalizedUrl = NormalizeAbsoluteUrlNoQueryNoFragment(url);

        if (!visitedPages.TryAdd(normalizedUrl, 0))
            return;

        string html;
        try
        {
            html = await httpClient.GetStringAsync(url, ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to load page {Url}", url);
            return;
        }

        var doc = await browsingContext.OpenAsync(req => req.Content(html), ct);

        var title = doc.QuerySelector("title")?.TextContent?.Trim() ?? "(No Page Title)";
        var pageDisplayTitle = doc.QuerySelector("meta[name='phaeno:search-title']")?.GetAttribute("content")?.Trim() ?? title;
        var description = doc.QuerySelector("meta[name='description']")?.GetAttribute("content")?.Trim() ?? "";
        var documentType = doc.QuerySelector("meta[name='phaeno:document-type']")?.GetAttribute("content")?.Trim() ?? "page";
        var pageKeywords = doc.QuerySelector("meta[name='phaeno:search-keywords']")?.GetAttribute("content")?.Trim() ?? "";

        var main = doc.QuerySelector("main") ?? doc.Body;
        if (main is null)
            return;

        var bodyText = HtmlTextExtractor.ExtractCleanText(main);

        // Sections: index each heading with an id as a separate anchor
        var headings = main.QuerySelectorAll("h1[id], h2[id], h3[id], h4[id], h5[id], h6[id], [id][data-phaeno-search]");
        if (headings.Length > 0)
        {
            foreach (var heading in headings)
            {
                var id = heading.GetAttribute("id");
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var headingText = heading.TextContent?.Trim();
                if (string.IsNullOrWhiteSpace(headingText))
                    continue;

                var anchorTitle =
                    heading.GetAttribute("data-phaeno-search")?.Trim()
                    ?? headingText;

                var sectionSummary = heading.GetAttribute("data-phaeno-search-summary")?.Trim();
                var sectionKeywords = heading.GetAttribute("data-phaeno-search-keywords")?.Trim() ?? "";

                var sectionText = HtmlTextExtractor.ExtractSectionText(heading, anchorTitle);

                results.Add(new IndexedPage
                {
                    Url = normalizedUrl + "#" + id,
                    PageTitle = title,
                    PageDisplayTitle = pageDisplayTitle,
                    Anchor = id,
                    AnchorTitle = anchorTitle,
                    Description = sectionSummary ?? "",
                    DocumentType = "section",
                    SearchKeywords = sectionKeywords,
                    Text = sectionText,
                    IndexedAt = DateTime.UtcNow
                });
            }
        }
        else
        {
            results.Add(new IndexedPage
            {
                Url = normalizedUrl,
                PageTitle = title,
                PageDisplayTitle = pageDisplayTitle,
                Description = description,
                DocumentType = documentType,
                SearchKeywords = pageKeywords,
                Text = bodyText,
                IndexedAt = DateTime.UtcNow
            });
        }
    }

    private static string NormalizeAbsoluteUrlNoQueryNoFragment(Uri uri)
    {
        // Keep scheme + host + path. Drop query + fragment.
        var normalized = uri.GetLeftPart(UriPartial.Path).TrimEnd('/');
        return normalized.ToLowerInvariant();
    }

    private static string NormalizeAbsoluteUrlNoQueryNoFragment(string url)
        => NormalizeAbsoluteUrlNoQueryNoFragment(new Uri(url));

}
