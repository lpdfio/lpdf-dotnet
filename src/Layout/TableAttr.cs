// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Rows and cells in columns whose widths are set by cols. The thead row repeats at the top of every page, and rows move between pages whole.</summary>
public sealed record TableAttr
{
    /// <summary>Column widths, separated by spaces, in fr, pt or % units: for example 2fr 1fr 120pt 20%.</summary>
    public required string Cols { get; init; }
    public string? Border { get; init; }
    public string? Stripe { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Width { get; init; }
    public string? Height { get; init; }
    public string? Paginate { get; init; }
    public string? Debug { get; init; }
}
