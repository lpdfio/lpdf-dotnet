namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes for the <c>region</c> primitive.</summary>
public sealed record RegionAttr(
    string  Pin,
    string? Page  = null,
    string? W     = null,
    string? Debug = null);
