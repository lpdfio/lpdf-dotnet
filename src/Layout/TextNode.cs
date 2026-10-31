namespace Lpdf.Layout;

/// <summary>A serialised <c>text</c> paragraph node.</summary>
public sealed record TextNode(
    Dictionary<string, object> Attrs,
    List<Content>              Children) : Node
{
    /// <inheritdoc/>
    public override string Type => "text";
}
