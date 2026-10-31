namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>An <c>img-at</c> in a layer.</summary>
public sealed record ImgAtNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "img-at";
}
