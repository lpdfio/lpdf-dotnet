namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>An <c>ellipse</c> on the canvas.</summary>
public sealed record EllipseNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "ellipse";
}
