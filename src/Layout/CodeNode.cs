namespace Lpdf.Layout;

/// <summary>A serialised <c>code</c> node: code in a box, whose strings keep their spaces and lines.</summary>
public sealed record CodeNode(
    Dictionary<string, object> Attrs,
    List<Content>              Children) : Node
{
    /// <inheritdoc/>
    public override string Type => "code";
}
