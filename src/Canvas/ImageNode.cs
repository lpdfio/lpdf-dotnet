namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>An <c>img</c> in a layer.</summary>
public sealed record ImageNode(Dictionary<string, string> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "img";
}
