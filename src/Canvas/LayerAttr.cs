namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>Style attributes for <c>canvas-layer</c> nodes.</summary>
public sealed record LayerAttr(
    string?    Page      = null,
    double?    Opacity   = null,
    Transform? Transform = null,
    Clip?      Clip      = null);
