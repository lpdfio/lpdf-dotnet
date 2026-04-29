namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes for the <c>region</c> primitive.</summary>
public sealed record RegionAttr(
    string? X          = null, string? Y      = null,
    string? Width      = null, string? Height = null,
    string? Page       = null, string? Debug  = null);
