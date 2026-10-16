// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>link</c> element.</summary>
public sealed record LinkAttr
{
    /// <summary>Where the link goes: a URL, or #id to jump to the bookmark with that id. In an entry of a table of contents, # alone jumps to that entry's bookmark.</summary>
    public required string Href { get; init; }
    public string? Gap { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Debug { get; init; }
}
