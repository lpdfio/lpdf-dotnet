namespace Lpdf.Layout;

/// <summary>A serialised <c>code-span</c> inline node: a run of code in a <c>text</c>.</summary>
public sealed record CodeSpanNode(
    Dictionary<string, object> Attrs,
    List<string>               Children) : Node, Content
{
    /// <inheritdoc/>
    public override string Type => "code-span";
}
