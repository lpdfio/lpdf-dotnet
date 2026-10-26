// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>An item of a bullet or a numbered list. indent is the width of the marker's column and bullet the shape of a bullet list's marker, as on a list; gap goes between the blocks of an item.</summary>
public sealed record MarkdownLiAttr
{
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public string? Color { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Indent { get; init; }
    public string? Bullet { get; init; }
}
