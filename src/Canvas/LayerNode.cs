namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>layer</c> on the canvas, holding canvas primitives.</summary>
public sealed record LayerNode(
    Dictionary<string, string> Attrs,
    List<CanvasNode>           Nodes) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "layer";
}
