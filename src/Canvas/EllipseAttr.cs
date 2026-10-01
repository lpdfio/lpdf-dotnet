// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>ellipse</c> element on the canvas.</summary>
public sealed record EllipseAttr
{
    public required string Rx { get; init; }
    public required string Ry { get; init; }
    public string? Cx { get; init; }
    public string? Cy { get; init; }
    public string? Anchor { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }
    public string? Opacity { get; init; }
}
