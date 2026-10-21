// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>A QR code, a Code 128 or an EAN-13. It is never stretched. With only a width or a height, the other follows the barcode's proportions. With both, they are a maximum: the barcode takes the largest size that fits in them, and its box becomes that size, which the parent's align places. To put barcodes in slots of one size, wrap each in a frame of that size.</summary>
public sealed record BarcodeAttr
{
    public required string Type { get; init; }
    public required string Data { get; init; }

    /// <summary>Height of the whole barcode, a length or a name from the height row of the tokens, with the digits of hrt inside it: they take the bottom 12pt and the bars the rest. A barcode is never stretched. With only a height, the width follows the barcode's proportions. With both width and height, they are a maximum: the barcode takes the largest size that fits in them, and its box becomes that size. A QR code is square, so it takes the smaller of the two. Without a size, a code128 is as wide as its bars at 1pt per module and its bars are 40pt high, and an ean13 is the standard symbol, 0.33mm per module and bars 22.85mm high. The digits of hrt add 12pt to either.</summary>
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
