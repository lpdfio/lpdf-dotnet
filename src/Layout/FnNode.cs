namespace Lpdf.Layout;

/// <summary>
/// A serialised <c>fn</c> footnote inline node. The engine numbers it and puts a small raised number at its
/// place in the text; its words go at the bottom of the page where that number lands.
/// </summary>
public sealed record FnNode(
    Dictionary<string, object> Attrs,
    List<Content>              Children) : Node, Content
{
    /// <inheritdoc/>
    public override string Type => "fn";
}
