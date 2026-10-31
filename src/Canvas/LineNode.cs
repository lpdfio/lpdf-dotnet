namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>line</c> in a layer.</summary>
public sealed record LineNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "line";
}
