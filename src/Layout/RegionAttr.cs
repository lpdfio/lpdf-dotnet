// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Pins content to the top or bottom edge of pages, outside the normal flow, for headers and footers. It reserves that space on every page it appears on. Only allowed directly inside layout. Where it is written among the flow does not matter: pin chooses the edge, and regions on one edge stack in the order written.</summary>
public sealed record RegionAttr
{
    /// <summary>Which edge the region sticks to. top and bottom work. left and right pass validation but are not implemented yet.</summary>
    public required string Pin { get; init; }

    /// <summary>Which pages show the region: each, first, last, odd, even, or a range such as 2-last. odd and even follow the number printed on the page, so in a section that starts on page 4 even shows on its first page. first, last and ranges count the section's own pages, whatever their numbers. Defaults to each.</summary>
    public string? Page { get; init; }
    public string? W { get; init; }
    public bool? Debug { get; init; }
}
