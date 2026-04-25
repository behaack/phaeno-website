namespace phaeno.api.Infrastructure.WebcrawlerServices.SupportFiles;

/// <summary>
/// Minimal robots.txt implementation for "allow / disallow" and Sitemap discovery.
/// - Assumes allowed if robots.txt is missing/unreachable.
/// - Parses Disallow rules for User-agent: *
/// - Extracts Sitemap: URLs for crawler guidance.
/// </summary>
public sealed class RobotsTxtHelper
{
    private readonly HttpClient httpClient;
    private readonly string userAgent;

    private readonly HashSet<string> disallowedPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<Uri> sitemaps = new();

    public IReadOnlyList<Uri> Sitemaps => sitemaps;

    public RobotsTxtHelper(HttpClient httpClient, string? userAgent = null)
    {
        this.httpClient = httpClient;
        this.userAgent = string.IsNullOrWhiteSpace(userAgent) ? "*" : userAgent.Trim();
    }

    public async Task LoadAsync(Uri root, CancellationToken ct = default)
    {
        disallowedPaths.Clear();
        sitemaps.Clear();

        try
        {
            var robotsUrl = new Uri(root, "/robots.txt");
            var content = await httpClient.GetStringAsync(robotsUrl, ct);

            using var reader = new StringReader(content);

            string? line;
            bool groupApplies = false;
            bool seenAnyUserAgentInGroup = false;
            var currentAgents = new List<string>();

            while ((line = reader.ReadLine()) != null)
            {
                line = StripComments(line).Trim();
                if (line.Length == 0)
                    continue;

                // Handle Sitemap: lines (not group-scoped)
                if (line.StartsWith("Sitemap:", StringComparison.OrdinalIgnoreCase))
                {
                    var raw = line.Split(':', 2)[1].Trim();
                    if (Uri.TryCreate(raw, UriKind.Absolute, out var sm))
                        sitemaps.Add(sm);
                    else if (Uri.TryCreate(root, raw, out var relSm))
                        sitemaps.Add(relSm);

                    continue;
                }

                // Split directive:value
                var colon = line.IndexOf(':');
                if (colon <= 0)
                    continue;

                var key = line[..colon].Trim();
                var value = line[(colon + 1)..].Trim();

                if (key.Equals("User-agent", StringComparison.OrdinalIgnoreCase))
                {
                    // New group starts when a user-agent appears after we already had directives in the group.
                    if (seenAnyUserAgentInGroup && currentAgents.Count > 0 && groupApplies == false && value.Length > 0)
                    {
                        // no-op: keep reading; minimal parser doesn’t need explicit group boundaries
                    }

                    seenAnyUserAgentInGroup = true;

                    if (!string.IsNullOrWhiteSpace(value))
                        currentAgents.Add(value);

                    // Minimal: treat group as applicable if it contains "*" (or matches our UA loosely).
                    groupApplies = currentAgents.Any(a =>
                        a == "*" ||
                        (this.userAgent != "*" && this.userAgent.Contains(a, StringComparison.OrdinalIgnoreCase)));

                    continue;
                }

                if (key.Equals("Disallow", StringComparison.OrdinalIgnoreCase))
                {
                    if (!groupApplies)
                        continue;

                    // Empty disallow means "allow all"
                    if (string.IsNullOrWhiteSpace(value))
                        continue;

                    // Ensure starts with '/'
                    if (!value.StartsWith("/"))
                        value = "/" + value;

                    disallowedPaths.Add(value);
                }

                // (Optional) Allow is ignored in this minimal helper because your robots has Allow: /
                // and you have no Disallow rules. If you later add Disallow + Allow exceptions,
                // switch to a full "longest match wins" implementation.
            }
        }
        catch
        {
            // Minimal robots.txt implementation: assume allowed if unavailable
        }
    }

    public bool IsAllowed(string absolutePath)
    {
        if (string.IsNullOrWhiteSpace(absolutePath))
            return true;

        if (!absolutePath.StartsWith("/"))
            absolutePath = "/" + absolutePath;

        foreach (var disallowed in disallowedPaths)
        {
            if (absolutePath.StartsWith(disallowed, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    private static string StripComments(string line)
    {
        var idx = line.IndexOf('#');
        return idx >= 0 ? line[..idx] : line;
    }
}
