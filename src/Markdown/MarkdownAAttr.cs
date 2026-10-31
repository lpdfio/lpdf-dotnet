// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A link, written [text](url) in Markdown.</summary>
public sealed record MarkdownAAttr
{
    public string? Font { get; init; }
    public string? Color { get; init; }
    public bool? Bold { get; init; }
    public bool? Underline { get; init; }
}
