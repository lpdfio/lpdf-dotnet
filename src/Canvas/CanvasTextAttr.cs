// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Text at an exact position on the page: either x and y, or an anchor with optional offsets.</summary>
public sealed record CanvasTextAttr
{
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public string? Color { get; init; }
    public string? Align { get; init; }
    public string? W { get; init; }
    public string? LineHeight { get; init; }
    public string? Opacity { get; init; }
}
