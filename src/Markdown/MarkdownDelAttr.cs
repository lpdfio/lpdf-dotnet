// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>Struck-through text, written ~~text~~ in Markdown.</summary>
public sealed record MarkdownDelAttr
{
    public string? Font { get; init; }
    public string? Color { get; init; }
    public bool? Strike { get; init; }
}
