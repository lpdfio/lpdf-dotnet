// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>path</c> element on the canvas.</summary>
public sealed record PathAttr
{
    public required string D { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? FillRule { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }
    public string? LineCap { get; init; }
    public string? Opacity { get; init; }
}
