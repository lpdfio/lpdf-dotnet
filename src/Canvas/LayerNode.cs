using Lpdf.Kit;

namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>layer</c>, holding the shapes, text and images drawn at fixed places on the page.</summary>
public sealed record LayerNode(
    Dictionary<string, string> Attrs,
    List<CanvasNode>           Nodes) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "layer";

    /// <summary>The layer as a child of a section, so <see cref="L.Layer"/> goes in a section's content.</summary>
    public static implicit operator SectionContent(LayerNode layer) => new SectionLayer(layer);
}
