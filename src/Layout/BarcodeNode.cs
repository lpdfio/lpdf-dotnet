namespace Lpdf.Layout;

/// <summary>A serialised <c>barcode</c> node.</summary>
public sealed record BarcodeNode(
    Dictionary<string, object> Attrs) : Node
{
    /// <inheritdoc/>
    public override string Type => "barcode";
}
