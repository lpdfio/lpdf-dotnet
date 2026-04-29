namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes for the <c>barcode</c> primitive.</summary>
public sealed record BarcodeAttr(
    string  Type,              string  Data,
    string? Size       = null, string? Width      = null,
    string? Height     = null, string? Ec         = null,
    string? Hrt        = null, string? Color      = null,
    string? Background = null, string? Repeat     = null,
    string? Debug      = null);
