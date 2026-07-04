using Lucene.Net.Analysis;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using Microsoft.Extensions.Options;
using phaeno.api.Configuration;
using phaeno.api.Infrastructure.WebSearchServices.SupportingFiles;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Directory = Lucene.Net.Store.Directory;

namespace phaeno.api.Infrastructure.WebSearchServices;

public class WebSearchService : IWebSearchService
{
    private static readonly LuceneVersion AppLuceneVersion = LuceneVersion.LUCENE_48;
    private readonly Directory _directory;
    private readonly Analyzer _analyzer = new StemmingAnalyzer();

    public WebSearchService(IWebHostEnvironment hostEnvironment, IOptions<WebSearchSettings> settings)
    {
        string searchFile = Path.Combine(hostEnvironment.ContentRootPath, settings.Value.SearchIndexLocation);
        _directory = FSDirectory.Open(searchFile);
    }

    public void DeleteStaleDocuments(IEnumerable<string> liveUrls)
    {
        var liveSet = new HashSet<string>(liveUrls);
        using var writer = new IndexWriter(_directory, new IndexWriterConfig(AppLuceneVersion, _analyzer));
        var reader = DirectoryReader.Open(writer, true);

        foreach (var leaf in reader.Leaves)
        {
            var atomicReader = (AtomicReader)leaf.Reader;
            for (int i = 0; i < atomicReader.MaxDoc; i++)
            {
                var doc = atomicReader.Document(i);
                var url = doc.Get("url");
                if (!liveSet.Contains(url))
                {
                    writer.DeleteDocuments(new Term("url", url));
                }
            }
        }

        writer.Commit();
    }

    public IEnumerable<string> GetAllIndexedUrls()
    {
        var urls = new List<string>();
        using var reader = DirectoryReader.Open(_directory);
        for (int i = 0; i < reader.MaxDoc; i++)
        {
            var doc = reader.Document(i);
            var url = doc.Get("url");
            if (!string.IsNullOrWhiteSpace(url))
                urls.Add(url);
        }
        return urls;
    }

    public void RebuildIndex(IEnumerable<IndexedPage> newPages)
    {
        using var writer = new IndexWriter(_directory, new IndexWriterConfig(AppLuceneVersion, _analyzer) { OpenMode = OpenMode.CREATE });

        var distinctPages = newPages
            .Where(w => w.DocumentType != "List")
            .ToList();

        foreach (var page in distinctPages)
        {
            var doc = PrepareDocumentWithStemming(page);
            writer.AddDocument(doc);
        }

        writer.Commit();
    }

    public List<IndexedPage> Search(string queryText)
    {
        const int HIT_COUNT = 30;

        var stemmedTerms = Regex.Matches(queryText, "\\b[\\w']+\\b")
            .Cast<Match>()
            .Select(m => NormalizeAndStem(m.Value))
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .ToList();

        var builder = new BooleanQuery();
        foreach (var term in stemmedTerms)
        {
            builder.Add(new TermQuery(new Term("stemmedText", term)), Occur.MUST);
        }

        using var reader = DirectoryReader.Open(_directory);
        var searcher = new IndexSearcher(reader);
        var hits = searcher.Search(builder, HIT_COUNT);

        var results = hits.ScoreDocs.Select(hit =>
        {
            var doc = searcher.Doc(hit.Doc);
            string fullText = doc.Get("text") ?? "";
            string pageTitle = doc.Get("pageTitle") ?? "";
            string pageDisplayTitle = doc.Get("pageDisplayTitle") ?? pageTitle;
            string description = doc.Get("description") ?? "";
            string metadataText = string.Join(" ", new[]
            {
                pageTitle,
                pageDisplayTitle,
                doc.Get("anchorTitle"),
                description,
                doc.Get("searchKeywords")
            }.Where(value => !string.IsNullOrWhiteSpace(value)));

            int matchCount = CountStemmedMatches($"{fullText} {metadataText}", stemmedTerms);
            string snippet = ExtractSnippet(fullText, stemmedTerms);
            if (string.IsNullOrWhiteSpace(snippet))
                snippet = ExtractSnippet(description, stemmedTerms);
            if (string.IsNullOrWhiteSpace(snippet))
                snippet = TruncateSnippet(description, 200);

            return new IndexedPage
            {
                Id = doc.Get("id"),
                Url = doc.Get("url"),
                PageTitle = pageTitle,
                PageDisplayTitle = pageDisplayTitle,
                Anchor = doc.Get("anchor"),
                AnchorTitle = doc.Get("anchorTitle"),
                Text = fullText,
                Description = description,
                DocumentType = doc.Get("documentType"),
                Snippet = snippet,
                Score = hit.Score,
                Count = matchCount,
                IndexedAt = new DateTime(long.Parse(doc.Get("indexedAt")))
            };
        }).ToList();

        results = results
            .Where(w => w.Count > 0)
            .GroupBy(r => string.IsNullOrWhiteSpace(r.PageDisplayTitle) ? r.PageTitle : r.PageDisplayTitle)
            .OrderByDescending(g => g.Sum(r => r.Count ?? 0))
            .ThenByDescending(g => g.Max(r => r.Score ?? 0))
            .ThenBy(g => g.Key)
            .SelectMany(g => g
                .OrderByDescending(r => r.Count ?? 0)
                .ThenByDescending(r => r.Score)
                .ThenBy(r => r.AnchorTitle))
            .ToList();

        return results;
    }

    private Document PrepareDocumentWithStemming(IndexedPage page)
    {
        var doc = new Document();

        var searchableText = string.Join(" ", new[]
        {
            page.PageTitle,
            page.PageDisplayTitle,
            page.AnchorTitle,
            page.Description,
            page.SearchKeywords,
            page.Text
        }.Where(value => !string.IsNullOrWhiteSpace(value)));

        var stemmedWords = Regex.Matches(searchableText, "\\b[\\w']+\\b")
            .Cast<Match>()
            .Select(m => NormalizeAndStem(m.Value))
            .Where(term => !string.IsNullOrWhiteSpace(term))
            .Distinct();

        string stemmedText = string.Join(" ", stemmedWords);

        doc.Add(new StringField("id", page.Id ?? "", Field.Store.YES));
        doc.Add(new StringField("url", page.Url ?? "", Field.Store.YES));
        doc.Add(new TextField("pageTitle", page.PageTitle ?? "", Field.Store.YES));
        doc.Add(new TextField("pageDisplayTitle", page.PageDisplayTitle ?? page.PageTitle ?? "", Field.Store.YES));
        doc.Add(new TextField("text", page.Text ?? "", Field.Store.YES));
        doc.Add(new TextField("anchor", page.Anchor ?? "", Field.Store.YES));
        doc.Add(new TextField("anchorTitle", page.AnchorTitle ?? "", Field.Store.YES));
        doc.Add(new TextField("stemmedText", stemmedText ?? "", Field.Store.NO));
        doc.Add(new StoredField("description", page.Description ?? ""));
        doc.Add(new StoredField("documentType", page.DocumentType ?? ""));
        doc.Add(new StoredField("searchKeywords", page.SearchKeywords ?? ""));
        doc.Add(new StoredField("indexedAt", page.IndexedAt.Ticks.ToString()));

        return doc;
    }

    private int CountStemmedMatches(string text, List<string> stemmedQueries)
    {
        if (string.IsNullOrWhiteSpace(text) || stemmedQueries.Count == 0)
            return 0;

        return Regex.Matches(text, "\\b[\\w']+\\b")
                    .Cast<Match>()
                    .Select(m => NormalizeAndStem(m.Value))
                    .Count(normalized => stemmedQueries.Contains(normalized, StringComparer.OrdinalIgnoreCase));
    }

    private string ExtractSnippet(string text, List<string> stemmedQueries, int maxLength = 200, int windowSize = 100)
    {
        if (string.IsNullOrWhiteSpace(text) || stemmedQueries.Count == 0)
            return "";

        // Normalize _queries to lower for case-insensitive matching
        var lowerQueries = stemmedQueries.Select(q => q.ToLowerInvariant()).ToHashSet();

        // Tokenize text into words with positions
        var words = new List<(string Original, string Stem, int StartIndex, int Length)>();
        int index = 0;
        while (index < text.Length)
        {
            // Skip whitespace
            while (index < text.Length && char.IsWhiteSpace(text[index])) index++;
            if (index >= text.Length) break;

            // Find word boundaries (simple: letters/digits, but customizable)
            int start = index;
            while (index < text.Length && !char.IsWhiteSpace(text[index])) index++;
            string word = text.Substring(start, index - start);
            string clean = word.Trim('\'', '.', ',', ';', ':', '?', '!', '"', ')', '(');
            string stem = NormalizeAndStem(clean).ToLowerInvariant();

            words.Add((word, stem, start, word.Length));
        }

        // Find matching positions
        var matches = words.Where(w => lowerQueries.Contains(w.Stem))
                           .Select(w => (Start: w.StartIndex, End: w.StartIndex + w.Length))
                           .ToList();

        if (matches.Count == 0)
        {
            // Improved fallback: Case-insensitive substring search for any query as last resort
            foreach (var q in stemmedQueries)
            {
                int pos = text.IndexOf(q, StringComparison.OrdinalIgnoreCase);
                if (pos >= 0)
                {
                    return TruncateSnippet(text.Substring(pos, Math.Min(text.Length - pos, maxLength)), maxLength);
                }
            }
            return "";
        }

        // For simplicity, build snippet around the first match (extend to clusters if needed)
        var firstMatch = matches.First();
        int snippetStart = Math.Max(0, firstMatch.Start - windowSize);
        // Adjust to start of word or sentence for better context
        snippetStart = text.LastIndexOf(' ', snippetStart) + 1; // Start at word boundary
        int snippetEnd = Math.Min(text.Length, firstMatch.End + windowSize);
        snippetEnd = text.IndexOf(' ', snippetEnd);
        if (snippetEnd == -1) snippetEnd = text.Length; // End at word boundary

        string snippet = text.Substring(snippetStart, snippetEnd - snippetStart);

        // Highlight all matches in the snippet
        var highlighted = new System.Text.StringBuilder(snippet);
        // Sort matches descending to avoid index shifts
        foreach (var match in matches.Where(m => m.Start >= snippetStart && m.End <= snippetEnd).OrderByDescending(m => m.Start))
        {
            int relStart = match.Start - snippetStart;
            highlighted.Insert(relStart + (match.End - match.Start), "}}");
            highlighted.Insert(relStart, "{{");
        }

        return TruncateSnippet(highlighted.ToString(), maxLength);
    }

    private string TruncateSnippet(string snippet, int maxLength)
    {
        if (snippet.Length <= maxLength) return snippet;
        // Truncate to last space before maxLength
        int lastSpace = snippet.LastIndexOf(' ', maxLength);
        return (lastSpace > 0 ? snippet.Substring(0, lastSpace) : snippet.Substring(0, maxLength)) + "...";
    }

    string NormalizeAndStem(string word)
    {
        var stemmer = new PorterStemmer();

        word = RemoveAccents(word.ToLowerInvariant());
        word = Regex.Replace(word, "'s\\b", "");
        word = Regex.Replace(word, "[^\\w\\s]", "");
        word = word.Replace("-", " ");

        if (Regex.IsMatch(word, @"\d{3,}") || !Regex.IsMatch(word, @"[a-zA-Z]"))
            return "";

        return stemmer.Stem(word);
    }

    public static string RemoveAccents(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        var normalized = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();

        foreach (char c in normalized)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }
}
