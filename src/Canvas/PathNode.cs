namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>path</c> on the canvas.</summary>
public sealed record PathNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "path";
}
