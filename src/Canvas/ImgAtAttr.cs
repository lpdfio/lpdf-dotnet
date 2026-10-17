// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>An image at a fixed place on the page. It needs width or height, and the other follows the image's proportions. With both, they are a maximum: the image takes the largest size that fits in them, and its box becomes that size, so an anchor places the image itself. x and y place its box: the top-left corner, or with anchor, an offset from that point of the page, where the box's matching point goes.</summary>
public sealed record ImgAtAttr
{
    public required string Name { get; init; }
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Width { get; init; }
    public string? Height { get; init; }
}
