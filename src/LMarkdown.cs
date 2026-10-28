using Lpdf.Markdown;
using Lpdf.Shared;

namespace Lpdf;

/// <summary>
/// The parts of a markdown: its styles, its content and the rules of its styles. The markdown itself is
/// <see cref="L.Markdown"/>.
///
/// <example>
/// <code>
/// L.Markdown(new() { Margin = "54pt", Gap = "m" }, [
///     LMarkdown.Styles(NoAttr, [LMarkdown.H1(new() { Color = "muted" })]),
///     LMarkdown.Content(NoAttr, [body]),
/// ])
/// </code>
/// </example>
/// </summary>
public static class LMarkdown
{
    /// <summary>
    /// Build the rules that say how the parts of a markdown look. A rule is named after the Markdown it styles and
    /// takes the attributes of the element it becomes. A rule here overrides the same rule of the file that
    /// <c>Src</c> names, which overrides the defaults.
    /// </summary>
    public static MarkdownStylesNode Styles(MarkdownStylesAttr? attrs = null, MarkdownRule[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>
    /// Build the Markdown, as strings that make one text. The common indent of its lines is removed, so that
    /// indenting the code does not make a code block of it. It has no attributes: pass <see cref="L.NoAttr"/>.
    /// </summary>
    public static MarkdownContentNode Content(NoAttributes? attrs = null, string[]? nodes = null)
        => new((nodes ?? []).ToList());

    /// <summary>Build the rule of a heading of level 1.</summary>
    public static MarkdownRule H1(MarkdownH1Attr? attrs = null) => Rule("h1", attrs);

    /// <summary>Build the rule of a heading of level 2.</summary>
    public static MarkdownRule H2(MarkdownH2Attr? attrs = null) => Rule("h2", attrs);

    /// <summary>Build the rule of a heading of level 3.</summary>
    public static MarkdownRule H3(MarkdownH3Attr? attrs = null) => Rule("h3", attrs);

    /// <summary>Build the rule of a heading of level 4.</summary>
    public static MarkdownRule H4(MarkdownH4Attr? attrs = null) => Rule("h4", attrs);

    /// <summary>Build the rule of a heading of level 5.</summary>
    public static MarkdownRule H5(MarkdownH5Attr? attrs = null) => Rule("h5", attrs);

    /// <summary>Build the rule of a heading of level 6.</summary>
    public static MarkdownRule H6(MarkdownH6Attr? attrs = null) => Rule("h6", attrs);

    /// <summary>Build the rule of a paragraph, and of the words of a list item, a quote and a footnote.</summary>
    public static MarkdownRule P(MarkdownPAttr? attrs = null) => Rule("p", attrs);

    /// <summary>Build the rule of code: the box of a block, and the run of inline code.</summary>
    public static MarkdownRule Code(MarkdownCodeAttr? attrs = null) => Rule("code", attrs);

    /// <summary>Build the rule of a block quote.</summary>
    public static MarkdownRule Quote(MarkdownQuoteAttr? attrs = null) => Rule("quote", attrs);

    /// <summary>Build the rule of an item of a bullet or a numbered list.</summary>
    public static MarkdownRule Li(MarkdownLiAttr? attrs = null) => Rule("li", attrs);

    /// <summary>Build the rule of a link.</summary>
    public static MarkdownRule A(MarkdownAAttr? attrs = null) => Rule("a", attrs);

    /// <summary>Build the rule of strong text.</summary>
    public static MarkdownRule Strong(MarkdownStrongAttr? attrs = null) => Rule("strong", attrs);

    /// <summary>Build the rule of emphasis.</summary>
    public static MarkdownRule Em(MarkdownEmAttr? attrs = null) => Rule("em", attrs);

    /// <summary>Build the rule of struck-through text.</summary>
    public static MarkdownRule Del(MarkdownDelAttr? attrs = null) => Rule("del", attrs);

    /// <summary>Build the rule of a table.</summary>
    public static MarkdownRule Table(MarkdownTableAttr? attrs = null) => Rule("table", attrs);

    /// <summary>Build the rule of a cell of a table's header row.</summary>
    public static MarkdownRule Th(MarkdownThAttr? attrs = null) => Rule("th", attrs);

    /// <summary>Build the rule of a cell of a table's other rows.</summary>
    public static MarkdownRule Td(MarkdownTdAttr? attrs = null) => Rule("td", attrs);

    private static MarkdownRule Rule(string type, object? attrs) => new(type, AttrsHelper.Attrs(attrs));
}
