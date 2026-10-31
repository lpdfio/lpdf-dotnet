// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A cell of a table's header row.</summary>
public sealed record MarkdownThAttr
{
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public bool? Bold { get; init; }
    public string? Color { get; init; }
    public string? Align { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
}
