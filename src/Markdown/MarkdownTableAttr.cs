// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A table. Its columns are sized by their content: a column takes the width its cells need on one line, the room that is left is shared in proportion, and a name that does not fit breaks at the edge of its cell.</summary>
public sealed record MarkdownTableAttr
{
    public string? Border { get; init; }
    public string? Stripe { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
}
