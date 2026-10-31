namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>circle</c> in a layer.</summary>
public sealed record CircleNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "circle";
}
