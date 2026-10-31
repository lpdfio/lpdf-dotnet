namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>An <c>ellipse</c> in a layer.</summary>
public sealed record EllipseNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "ellipse";
}
