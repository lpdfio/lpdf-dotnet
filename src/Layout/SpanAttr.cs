// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>span</c> element.</summary>
public sealed record SpanAttr
{
    public string? Font { get; init; }
    public string? Bold { get; init; }
    public string? Color { get; init; }
    public string? Href { get; init; }
    public string? Underline { get; init; }
    public string? Strike { get; init; }
}
