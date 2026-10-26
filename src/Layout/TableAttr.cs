// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Rows and cells in columns whose widths are set by cols. The thead row repeats at the top of every page, and rows move between pages whole.</summary>
public sealed record TableAttr
{
    /// <summary>Column widths, separated by spaces, in fr, pt or % units, or auto: for example 2fr 1fr 120pt 20%. An auto column takes the width its cells need on one line, and the auto columns share what the pt and % columns leave, in proportion to what they need. When they do not fit, each keeps at least its longest word, and when not even those fit, the longest are cut and a short column keeps its width. auto and fr cannot be in one table: use pt or % for the fixed columns. For example auto 90pt 60pt.</summary>
    public required string Cols { get; init; }
    public string? Border { get; init; }
    public string? Stripe { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Paginate { get; init; }
    public string? Debug { get; init; }
}
