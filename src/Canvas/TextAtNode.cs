using Lpdf.Layout;

namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A <c>text-at</c> in a layer. Its content is strings and <c>span</c> nodes, as in a layout <c>text</c>.</summary>
public sealed record TextAtNode(
    Dictionary<string, string> Attrs,
    List<Content>              Children) : CanvasNode
{
    /// <inheritdoc/>
    public override string Type => "text-at";
}
