// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>img</c> element on the canvas.</summary>
public sealed record CanvasImgAttr
{
    public required string Name { get; init; }
    public required string W { get; init; }
    public required string H { get; init; }
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
}
