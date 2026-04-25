using AngleSharp.Dom;
using System.Text;
using System.Text.RegularExpressions;

namespace phaeno.api.Infrastructure.WebcrawlerServices.SupportFiles;

public static class HtmlTextExtractor
{
    private static readonly string[] BlacklistSelectors =
    {
        "header", "footer", "script", "style", ".sr-only"
    };

    public static string ExtractCleanText(IElement element)
    {
        var clone = (IElement)element.Clone(true);

        foreach (var selector in BlacklistSelectors)
        {
            foreach (var node in clone.QuerySelectorAll(selector))
                node.Remove();
        }

        var sb = new StringBuilder();
        ExtractRecursive(clone, sb);

        var cleaned = Regex.Replace(sb.ToString(), @"(\r?\n\s*){2,}", "\n\n");
        return cleaned.Trim();
    }

    public static string ExtractSectionText(IElement heading)
    {
        var sb = new StringBuilder();
        sb.AppendLine(heading.TextContent.Trim());

        var current = heading.NextElementSibling;
        while (current != null && !IsHeading(current))
        {
            sb.AppendLine(ExtractCleanText(current));
            current = current.NextElementSibling;
        }

        return sb.ToString().Trim();
    }

    private static void ExtractRecursive(INode node, StringBuilder sb)
    {
        if (node is IText text)
        {
            var value = text.Text.Trim();
            if (!string.IsNullOrWhiteSpace(value))
            {
                if (sb.Length > 0 && !char.IsWhiteSpace(sb[^1]))
                    sb.Append(' ');
                sb.Append(value);
            }
        }

        foreach (var child in node.ChildNodes)
            ExtractRecursive(child, sb);

        if (node is IElement el &&
            el.LocalName is "p" or "div" or "section" or "article" or "main")
        {
            sb.AppendLine();
            sb.AppendLine();
        }
    }

    private static bool IsHeading(IElement el)
        => el.TagName.Length == 2 && el.TagName[0] == 'H';
}
