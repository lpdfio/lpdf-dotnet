// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>A barcode at a fixed place on the page. A QR code takes width or height. A code128 or ean13 needs a width. x and y place its box: the top-left corner, or with anchor, an offset from that point of the page, where the box's matching point goes.</summary>
public sealed record BarcodeAtAttr
{
    public required string Type { get; init; }
    public required string Data { get; init; }

    /// <summary>Height of the whole barcode, with the digits of hrt inside it: they take the bottom 12pt and the bars the rest. A QR code is square: it takes width or height, and the smaller when both are set. Without a height, code128 has bars 40pt high, and 12pt more with hrt, and ean13 is 60pt high.</summary>
    public string? Height { get; init; }
    public string? Ec { get; init; }
    public string? Hrt { get; init; }
    public string? Color { get; init; }
    public string? Background { get; init; }
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Width { get; init; }
}
