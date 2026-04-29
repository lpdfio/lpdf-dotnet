namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes for the <c>divider</c> primitive.</summary>
public sealed record DividerAttr(
    string? Color      = null, string? Thickness = null, string? Direction = null);
