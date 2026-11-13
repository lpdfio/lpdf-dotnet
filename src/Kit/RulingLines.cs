// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>One set of lines of a ruling, cut by a step or by a count in each direction: x for the lines that run down the page, y for the lines that run across it. A direction with neither has no lines, so step-y alone is ruled paper and step both is squared paper. A step and a count in the same direction is an error. Each set is drawn after the one before it, so a heavier set paints over a finer one.</summary>
public sealed record RulingLines
{
    /// <summary>The distance between two lines in both directions, a length such as 5mm. step-x and step-y take its place in their direction. A cell narrower than 2.5pt is an error.</summary>
    public string? Step { get; init; }

    /// <summary>The distance between two lines that run down the page, from left to right, a length such as 10mm.</summary>
    public string? StepX { get; init; }

    /// <summary>The distance between two lines that run across the page, from top to bottom, a length such as 8mm: ruled paper.</summary>
    public string? StepY { get; init; }

    /// <summary>Cuts the space into this many equal parts in both directions, from 1 to 100, so the lines land on its edges whatever its size. count-x and count-y take its place in their direction. A part narrower than 2.5pt is an error.</summary>
    public int? Count { get; init; }

    /// <summary>Cuts the space into this many equal columns, from 1 to 100.</summary>
    public int? CountX { get; init; }

    /// <summary>Cuts the space into this many equal rows, from 1 to 100.</summary>
    public int? CountY { get; init; }

    /// <summary>The color of the lines: hex, cmyk(), rgb(), hsl(), a color token, or a spot color or a tint of it. Defaults to line-muted.</summary>
    public string? Color { get; init; }

    /// <summary>The thickness of the lines, a length or a name from the border row of the tokens. Defaults to 0.4pt.</summary>
    public string? Thickness { get; init; }
}
