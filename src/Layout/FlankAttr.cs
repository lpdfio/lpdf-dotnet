// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>One row where the children keep their own width except one, which fills the rest. By default the last child fills; set end to true and the first fills. Never splits across pages.</summary>
public sealed record FlankAttr
{
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public string? Gap { get; init; }

    /// <summary>Space between the edge of the box and its content, written like CSS: one value for all sides, two for top-bottom and left-right, three for top, left-right and bottom, four for top, right, bottom and left.</summary>
    public string? Padding { get; init; }

    /// <summary>Height of the box. Leave it out to size to the content. A length fixes it; fill takes what is left after its siblings (shared equally if several use fill); full takes all the height available. Any of these stops the box splitting across pages.</summary>
    public string? Height { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Radius { get; init; }
    public string? Debug { get; init; }
    public string? Align { get; init; }

    /// <summary>false (the default): every child but the last keeps its own width at the left, and the last fills the rest. true: the first child fills, and the others keep their own width at the right.</summary>
    public string? End { get; init; }
    public string? Width { get; init; }
}
