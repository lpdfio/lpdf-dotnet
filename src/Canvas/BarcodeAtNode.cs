namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>barcode-at</c> in a layer.</summary>
public sealed record BarcodeAtNode(Dictionary<string, object> Attrs) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "barcode-at";
}
