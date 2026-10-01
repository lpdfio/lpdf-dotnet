// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>barcode</c> element.</summary>
public sealed record BarcodeAttr
{
    public required string Type { get; init; }
    public required string Data { get; init; }
    public string? Size { get; init; }
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Ec { get; init; }
    public string? Hrt { get; init; }
    public string? Color { get; init; }
    public string? Background { get; init; }
    public string? Debug { get; init; }
}
