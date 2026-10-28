// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A table. Its columns are sized by their content: a column takes the width its cells need on one line, the room that is left is shared in proportion, and a name that does not fit breaks at the edge of its cell. stripe is the color of every other row, or none; head-border is the line under the header row and row-border the line under each of the other rows, as thickness and color like border, or none. The default styles draw a line in line-muted under the header and under each row, with no stripe.</summary>
public sealed record MarkdownTableAttr
{
    public string? Border { get; init; }
    public string? Stripe { get; init; }
    public string? HeadBorder { get; init; }
    public string? RowBorder { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
}
