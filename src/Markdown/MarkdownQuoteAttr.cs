// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A block quote, written &gt; in Markdown: a bar beside its content. font, font-size, color, line-height and align set the words; padding, background, border and radius the box around the bar and the words; bar-color and bar-width the bar, a strip along the left of the box: bar-width is a length or a name from the space row of the tokens.</summary>
public sealed record MarkdownQuoteAttr
{
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public bool? Bold { get; init; }
    public string? Color { get; init; }
    public string? Align { get; init; }
    public LineHeight? LineHeight { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Radius { get; init; }
    public string? BarColor { get; init; }
    public string? BarWidth { get; init; }
}
