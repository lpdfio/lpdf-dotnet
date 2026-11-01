// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>rect</c> element in a layer.</summary>
public sealed record RectAttr
{
    public required string Width { get; init; }
    public required string Height { get; init; }
    public string? X { get; init; }
    public string? Y { get; init; }
    public string? Anchor { get; init; }
    public string? Radius { get; init; }
    public string? Fill { get; init; }
    public string? Stroke { get; init; }
    public string? StrokeWidth { get; init; }
    public string? StrokeDash { get; init; }

    /// <summary>A hard-edged shadow: a copy of the shape, moved right and down by the first two lengths (negative is left and up) and filled with the color, drawn under the shape. An opacity from 0 to 1 after the color fades the shadow alone: "2pt 3pt #000000 0.2". There is no blur. A shape with no fill shows the shadow only outside it.</summary>
    public string? Shadow { get; init; }

    /// <summary>Puts the element, and everything in it, in an optional content group, which the viewer shows or hides. print: printed, and not shown on screen. screen: shown on screen, and not printed. The element keeps its room either way, so nothing moves. An element with a group inside an element with another shows only where both show. Acrobat, Chrome and Firefox switch the groups; Preview and Safari show everything, on screen and in print. Hidden content is still in the file, where it can be selected and copied: it is not a way to keep anything secret. A link in print content is drawn and does nothing, and a form field can't be in print content: use a frame with a border for a box filled in by hand. With pdfa or pdfua on the output, everything shows. With pdfx, the file is the print version: screen content stays in it, off, and a reader can turn it on in a layers panel.</summary>
    public string? Ocg { get; init; }
}
