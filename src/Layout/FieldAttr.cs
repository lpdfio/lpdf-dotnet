// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>field</c> element.</summary>
public sealed record FieldAttr
{
    public required string Type { get; init; }
    public required string Name { get; init; }
    public string? Value { get; init; }
    public string? Label { get; init; }
    public string? Options { get; init; }
    public string? Group { get; init; }
    public string? Checked { get; init; }
    public string? Required { get; init; }
    public string? Readonly { get; init; }
    public string? MaxLen { get; init; }
    public string? ActionUrl { get; init; }
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Debug { get; init; }
}
