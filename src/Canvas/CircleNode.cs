namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>circle</c> on the canvas.</summary>
public sealed record CircleNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "circle";
}
