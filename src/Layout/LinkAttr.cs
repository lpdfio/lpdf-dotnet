namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes for the <c>link</c> layout primitive.</summary>
public sealed record LinkAttr(
    string? Url    = null, string? Gap    = null,
    string? Width  = null, string? Height = null);
