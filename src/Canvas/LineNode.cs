namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>line</c> on the canvas.</summary>
public sealed record LineNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "line";
}
