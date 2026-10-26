// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>Emphasis, written *text* in Markdown. Italic in a built-in font; an embedded font names its italic face in font.</summary>
public sealed record MarkdownEmAttr
{
    public string? Font { get; init; }
    public string? Color { get; init; }
}
