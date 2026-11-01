// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Rows and cells in columns whose widths are set by cols. The thead row repeats at the top of every page, and rows move between pages whole.</summary>
public sealed record TableAttr
{
    /// <summary>Column widths, separated by spaces, in fr, pt or % units, or auto: for example 2fr 1fr 120pt 20%. An auto column takes the width its cells need on one line, and the auto columns share what the pt and % columns leave, in proportion to what they need. When they do not fit, each keeps at least its longest word, and when not even those fit, the longest are cut and a short column keeps its width. auto and fr cannot be in one table: use pt or % for the fixed columns. For example auto 90pt 60pt.</summary>
    public required string Cols { get; init; }

    /// <summary>A grid: a line around every cell, as thickness and color, for example 1pt #cccccc.</summary>
    public string? Border { get; init; }

    /// <summary>The background of every other row below the header, or none.</summary>
    public string? Stripe { get; init; }

    /// <summary>The line under the header row, across the table, as thickness and color like border, or none: for example xs line.</summary>
    public string? HeadBorder { get; init; }

    /// <summary>The line under each row, across the table, as thickness and color like border, or none: for example xs line-muted. Under the header row, head-border takes its place when it is set. A line stops where a cell spans on into the next row.</summary>
    public string? RowBorder { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Paginate { get; init; }
    public bool? Debug { get; init; }

    /// <summary>Puts the element, and everything in it, in an optional content group, which the viewer shows or hides. print: printed, and not shown on screen. screen: shown on screen, and not printed. The element keeps its room either way, so nothing moves. An element with a group inside an element with another shows only where both show. Acrobat, Chrome and Firefox switch the groups; Preview and Safari show everything, on screen and in print. Hidden content is still in the file, where it can be selected and copied: it is not a way to keep anything secret. A link in print content is drawn and does nothing, and a form field can't be in print content: use a frame with a border for a box filled in by hand. With pdfa, pdfua or pdfx on the output, everything shows.</summary>
    public string? Ocg { get; init; }
}
