// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>One color of a gradient, at an offset along it.</summary>
public sealed record GradientStop
{
    /// <summary>Where the color is reached along the gradient, from 0% (the start) to 100% (the end). Stops go in increasing order.</summary>
    public required string Offset { get; init; }

    /// <summary>The color at the stop: hex, cmyk(), rgb(), hsl() or a color token. The stops of a gradient are all CMYK or all RGB.</summary>
    public required string Color { get; init; }
}
