namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>rect</c> on the canvas.</summary>
public sealed record RectNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "rect";
}
