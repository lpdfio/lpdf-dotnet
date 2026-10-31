namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>path</c> in a layer.</summary>
public sealed record PathNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "path";
}
