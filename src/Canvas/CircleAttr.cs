// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>circle</c> element in a layer.</summary>
public sealed record CircleAttr
{
    public required string R { get; init; }

    /// <summary>Where the circle's box goes, the square of side 2r around it: its left edge, or with anchor, an offset from that point of the page, where the box's matching point goes. The centre is r further in.</summary>
    public string? X { get; init; }

    /// <summary>Where the circle's box goes, the square of side 2r around it: its top edge, or with anchor, an offset from that point of the page, where the box's matching point goes. The centre is r further in.</summary>
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }
}
