namespace Lpdf.Layout;

/// <summary>A serialised <c>shape</c> node: a rect, circle or ellipse that takes its box in the layout.</summary>
public sealed record ShapeNode(
    Dictionary<string, object> Attrs) : Node
{
    /// <inheritdoc/>
    public override string Type => "shape";
}
