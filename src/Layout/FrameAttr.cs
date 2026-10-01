// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>A box around a single child, which it centres. Never splits across pages: if it does not fit, the whole box moves to the next page.</summary>
public sealed record FrameAttr
{
    public string? Font { get; init; }
    public string? FontSize { get; init; }

    /// <summary>Space between the edge of the box and its content, written like CSS: one value for all sides, two for top-bottom and left-right, three for top, left-right and bottom, four for top, right, bottom and left.</summary>
    public string? Padding { get; init; }

    /// <summary>Height of the box. Leave it out to size to the content. A length fixes it; fill takes what is left after its siblings (shared equally if several use fill); full takes all the height available. Any of these stops the box splitting across pages.</summary>
    public string? Height { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Radius { get; init; }
    public string? Debug { get; init; }
    public string? Width { get; init; }
}
