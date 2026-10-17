// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>ellipse</c> element in a layer.</summary>
public sealed record EllipseAttr
{
    public required string Rx { get; init; }
    public required string Ry { get; init; }

    /// <summary>Where the ellipse's box goes, 2rx wide and 2ry high around it: its left edge, or with anchor, an offset from that point of the page, where the box's matching point goes. The centre is rx further in.</summary>
    public string? X { get; init; }

    /// <summary>Where the ellipse's box goes, 2rx wide and 2ry high around it: its top edge, or with anchor, an offset from that point of the page, where the box's matching point goes. The centre is ry further in.</summary>
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }
    public string? Opacity { get; init; }
}
