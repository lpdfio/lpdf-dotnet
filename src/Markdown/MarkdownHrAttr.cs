// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A thematic break, written --- in Markdown: a divider. color is the color of the line and thickness its thickness, a length or a name from the border row of the tokens.</summary>
public sealed record MarkdownHrAttr
{
    public string? Color { get; init; }
    public string? Thickness { get; init; }
}
