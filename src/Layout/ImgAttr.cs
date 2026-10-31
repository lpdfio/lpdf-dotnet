// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>An image. It is never stretched. With only a width or a height, the other follows the image's proportions. With both, they are a maximum: the image takes the largest size that fits in them, and its box becomes that size, which the parent's align places. To put images of different shapes in slots of one size, wrap each in a frame of that size. With neither, the image has its natural size.</summary>
public sealed record ImgAttr
{
    public required string Name { get; init; }
    public string? Height { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Radius { get; init; }
    public string? Paginate { get; init; }
    public bool? Debug { get; init; }
}
