using Lpdf.Layout;

namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>Text on the canvas. Its content is strings and <c>span</c> nodes, as in a layout <c>text</c>.</summary>
public sealed record TextNode(
    Dictionary<string, string> Attrs,
    List<Content>              Children) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "text";
}
