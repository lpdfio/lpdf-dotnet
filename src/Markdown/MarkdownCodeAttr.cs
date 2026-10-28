// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>Code: the box of a fenced or indented block, and the run of inline code, which takes the font, the color and the background. The language of a fence is ignored, since there is no highlighting by language.</summary>
public sealed record MarkdownCodeAttr
{
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public string? Color { get; init; }
    public string? LineHeight { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Radius { get; init; }
    public string? TabSize { get; init; }
}
