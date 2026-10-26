// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Markdown;

#pragma warning disable CS1591
/// <summary>A table. The columns share the width equally.</summary>
public sealed record MarkdownTableAttr
{
    public string? Border { get; init; }
    public string? Stripe { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
}
