// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>rect</c> element in a layer.</summary>
public sealed record RectAttr
{
    public required string Width { get; init; }
    public required string Height { get; init; }
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Radius { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }

    /// <summary>A hard-edged shadow: a copy of the shape, moved right and down by the first two lengths (negative is left and up) and filled with the color, drawn under the shape. An opacity from 0 to 1 after the color fades the shadow alone: "2pt 3pt #000000 0.2". There is no blur. A shape with no fill shows the shadow only outside it.</summary>
    public string? Shadow { get; init; }
}
