namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>rect</c> in a layer.</summary>
public sealed record RectNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "rect";
}
