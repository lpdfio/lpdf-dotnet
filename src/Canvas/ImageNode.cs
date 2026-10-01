namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>An <c>img</c> on the canvas.</summary>
public sealed record ImageNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "img";
}
