// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>line</c> element on the canvas.</summary>
public sealed record LineAttr
{
    public required string X1 { get; init; }
    public required string Y1 { get; init; }
    public required string X2 { get; init; }
    public required string Y2 { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }
    public string? LineCap { get; init; }
}
