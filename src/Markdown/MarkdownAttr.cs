// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>Markdown as the content of a section, where a layout would go. It holds only Markdown: optional markdown-styles, which say how each part of it looks, and one markdown-content, which holds the text. The page, a header, a footer or a cover are not Markdown, so they stay in the XML: layers paint a header and a footer in the margin, and a cover or a table of contents is a section of its own. CommonMark with tables, strikethrough and footnotes; raw HTML is an error that names the line, and a $ is text. A heading is a bookmark, so the table of contents and the outline of the viewer fill themselves.</summary>
public sealed record MarkdownAttr
{
    /// <summary>The space between the page edges and the Markdown: one value for all four sides, or two to four in the order top, right, bottom, left. Defaults to 36pt. A layer draws a header or a footer in it.</summary>
    public string? Margin { get; init; }

    /// <summary>The font of the Markdown, which a rule can change. Defaults to the document's.</summary>
    public string? Font { get; init; }

    /// <summary>The size of the text of the Markdown, which a rule can change. Defaults to 11pt.</summary>
    public string? FontSize { get; init; }

    /// <summary>The color of the text of the Markdown, which a rule can change. Defaults to the text token.</summary>
    public string? Color { get; init; }

    /// <summary>The space between the blocks: headings, paragraphs, lists, code, quotes, tables and images. Default none.</summary>
    public string? Gap { get; init; }

    /// <summary>How many levels of headings become bookmarks, for the outline of the viewer and the table of contents: n keeps the levels 1 to n, 0 none. Default 6.</summary>
    public string? Bookmarks { get; init; }
    public string? Debug { get; init; }
}
