// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>barcode</c> element.</summary>
public sealed record BarcodeAttr
{
    public required string Type { get; init; }
    public required string Data { get; init; }

    /// <summary>Height of the whole barcode, with the digits of hrt inside it: they take the bottom 12pt and the bars the rest. A QR code is square: it takes width or height, and the smaller when both are set. Without a height, code128 has bars 40pt high, and 12pt more with hrt, and ean13 is 60pt high.</summary>
    public string? Height { get; init; }
    public string? Ec { get; init; }
    public string? Hrt { get; init; }
    public string? Color { get; init; }
    public string? Background { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }
    public string? Paginate { get; init; }
    public string? Debug { get; init; }
}
