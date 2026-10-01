// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>rect</c> element on the canvas.</summary>
public sealed record RectAttr
{
    public required string W { get; init; }
    public required string H { get; init; }
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Radius { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }
    public string? Opacity { get; init; }
}
