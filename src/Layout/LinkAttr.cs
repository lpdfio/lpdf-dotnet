// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>link</c> element.</summary>
public sealed record LinkAttr
{
    public required string Href { get; init; }
    public string? Gap { get; init; }
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Debug { get; init; }
}
