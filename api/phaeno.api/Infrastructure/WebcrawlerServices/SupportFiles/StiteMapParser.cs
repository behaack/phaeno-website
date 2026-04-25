using System.Xml.Linq;

namespace phaeno.api.Infrastructure.WebcrawlerServices.SupportFiles;

public static class SitemapParser
{
    public sealed record SitemapParseResult(
        List<Uri> Urls,
        List<Uri> ChildSitemaps
    );

    public static SitemapParseResult Parse(string xml, Uri root)
    {
        var doc = XDocument.Parse(xml, LoadOptions.None);

        // Default namespace in sitemaps is almost always present
        XNamespace ns = doc.Root?.Name.Namespace ?? "http://www.sitemaps.org/schemas/sitemap/0.9";

        var urls = new List<Uri>();
        var child = new List<Uri>();

        var rootName = doc.Root?.Name.LocalName;

        if (string.Equals(rootName, "sitemapindex", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var loc in doc.Descendants(ns + "sitemap").Elements(ns + "loc"))
            {
                var raw = loc.Value?.Trim();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                if (Uri.TryCreate(raw, UriKind.Absolute, out var sm))
                    child.Add(sm);
                else if (Uri.TryCreate(root, raw, out var relSm))
                    child.Add(relSm);
            }
        }
        else if (string.Equals(rootName, "urlset", StringComparison.OrdinalIgnoreCase))
        {
            foreach (var loc in doc.Descendants(ns + "url").Elements(ns + "loc"))
            {
                var raw = loc.Value?.Trim();
                if (string.IsNullOrWhiteSpace(raw))
                    continue;

                if (Uri.TryCreate(raw, UriKind.Absolute, out var u))
                    urls.Add(u);
                else if (Uri.TryCreate(root, raw, out var relU))
                    urls.Add(relU);
            }
        }

        return new SitemapParseResult(urls, child);
    }
}
