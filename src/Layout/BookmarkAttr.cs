// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>An entry of the PDF's outline, the bookmark panel of a viewer, and a target for links with href="#id". It draws nothing and marks the element that follows it, so it is never left at the bottom of a page apart from that element. Directly inside a section, it marks the section's first page.</summary>
public sealed record BookmarkAttr
{
    /// <summary>The text of the entry, in the outline and in a table of contents.</summary>
    public required string Label { get; init; }

    /// <summary>Depth in the outline, 1 to 6. Defaults to 1. An entry nests under the closest entry before it with a lower level.</summary>
    public int? Level { get; init; }

    /// <summary>The name links use to jump here, as href="#id". Leave it out to make it from the label: lowercase, with other characters than letters and digits turned into hyphens, and -2, -3 added when an earlier bookmark has it. Two bookmarks may not have the same id.</summary>
    public string? Id { get; init; }
}
