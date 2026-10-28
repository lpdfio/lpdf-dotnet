// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>Strong text, written **text** in Markdown. Bold in a built-in font; an embedded font names its bold face in font.</summary>
public sealed record MarkdownStrongAttr
{
    public string? Font { get; init; }
    public string? Color { get; init; }
    public string? Bold { get; init; }
}
