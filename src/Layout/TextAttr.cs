// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>A block of wrapping text. Use span children to style parts of it. Splits across pages between lines.</summary>
public sealed record TextAttr
{
    public string? FontSize { get; init; }
    public string? Font { get; init; }

    /// <summary>Use the bold face of the font. It applies to the 14 built-in fonts: Helvetica becomes Helvetica-Bold, Times-Roman becomes Times-Bold, Courier becomes Courier-Bold, and the oblique and italic faces their bold forms. A custom font has no bold face to pick, so name one in font.</summary>
    public string? Bold { get; init; }
    public string? Color { get; init; }
    public string? Align { get; init; }

    /// <summary>The distance between lines, as a factor of the font size: a number above 0, such as 1.5, or a name from the line-height row of the tokens. Default 1.2, which is the token m.</summary>
    public string? LineHeight { get; init; }

    /// <summary>The most lines drawn. Only whole lines are drawn, and the rest of the text is dropped. A text in the layout has no cap by default and wraps at the width it has; the lines it drops take no room, and a text that splits across pages keeps the cap across them. A text-at is one line by default, and above 1 needs a width to wrap within.</summary>
    public string? MaxLines { get; init; }

    /// <summary>End the last line drawn in an ellipsis when max-lines drops text, shortened to fit the width. Off by default: the text stops at the end of a line. A text-at also ends a line in one when it is cut off at its width. A text in the layout without max-lines is never cut, so there it has no effect.</summary>
    public string? Ellipsis { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }

    /// <summary>The fewest lines of the text that may stand alone at the top of the next page when it splits. Default 2. If fewer would be left, lines move from the first page to the next; 1 turns the protection off.</summary>
    public string? Widows { get; init; }

    /// <summary>The fewest lines of the text that may stand alone at the bottom of a page when it splits. Default 2. If fewer would fit, the whole text moves to the next page; 1 turns the protection off. When no split can keep both widows and orphans, even on an empty page, the text splits where the lines fit.</summary>
    public string? Orphans { get; init; }
    public string? Paginate { get; init; }
    public string? Debug { get; init; }
}
