// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Pins content to the top or bottom edge of pages, outside the normal flow, for headers and footers. It reserves that space on every page it appears on. Only allowed directly inside layout, and one per pin on a page.</summary>
public sealed record RegionAttr
{
    /// <summary>Which edge the region sticks to. top and bottom work. left and right pass validation but are not implemented yet.</summary>
    public required string Pin { get; init; }

    /// <summary>Which pages show the region: each, first, last, odd, even, or a range such as 2-last. Defaults to each.</summary>
    public string? Page { get; init; }
    public string? W { get; init; }
    public string? Debug { get; init; }
}
