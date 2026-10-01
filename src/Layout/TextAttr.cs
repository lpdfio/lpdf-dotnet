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
    public string? Width { get; init; }
    public string? Debug { get; init; }
}
