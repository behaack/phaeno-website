using AngleSharp.Dom;
using System.Text;
using System.Text.RegularExpressions;

namespace phaeno.api.Infrastructure.WebcrawlerServices.SupportFiles;

public static class HtmlTextExtractor
{
    private static readonly string[] BlacklistSelectors =
    {
        "header",
        "footer",
        "nav",
        "aside",
        "script",
        "style",
        "noscript",
        "form",
        "input",
        "textarea",
        "select",
        "button",
        "label",
        "svg",
        "[hidden]",
        "[aria-hidden='true']",
        "[data-phaeno-search-ignore]",
        ".sr-only",
        ".grecaptcha-badge"
    };

    public static string ExtractCleanText(IElement element)
    {
        var clone = (IElement)element.Clone(true);

        if (ShouldIgnore(clone))
            return "";

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

    public static string ExtractSectionText(IElement heading, string? searchTitle = null)
    {
        var sb = new StringBuilder();
        var headingText = string.IsNullOrWhiteSpace(searchTitle)
            ? heading.TextContent.Trim()
            : searchTitle.Trim();

        sb.AppendLine(headingText);

        var current = heading.NextElementSibling;
        while (current != null && !IsSectionBoundary(current))
        {
            if (!ShouldIgnore(current))
                sb.AppendLine(ExtractCleanText(current));
            current = current.NextElementSibling;
        }

        return sb.ToString().Trim();
    }

    private static void ExtractRecursive(INode node, StringBuilder sb)
    {
        if (node is IElement element && ShouldIgnore(element))
            return;

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

    private static bool ShouldIgnore(IElement element)
    {
        if (element.HasAttribute("data-phaeno-search-ignore"))
            return true;

        if (element.HasAttribute("hidden"))
            return true;

        if (string.Equals(element.GetAttribute("aria-hidden"), "true", StringComparison.OrdinalIgnoreCase))
            return true;

        if (element.ClassList.Contains("sr-only") || element.ClassList.Contains("grecaptcha-badge"))
            return true;

        return element.LocalName is
            "header" or
            "footer" or
            "nav" or
            "aside" or
            "script" or
            "style" or
            "noscript" or
            "form" or
            "input" or
            "textarea" or
            "select" or
            "button" or
            "label" or
            "svg";
    }

    private static bool IsSectionBoundary(IElement element)
    {
        if (element.TagName.Length == 2 && element.TagName[0] == 'H')
            return true;

        return element.HasAttribute("id") && element.HasAttribute("data-phaeno-search");
    }
}
