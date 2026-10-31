namespace Lpdf.Layout;

/// <summary>A serialised form <c>field</c> node.</summary>
public sealed record FieldNode(
    Dictionary<string, object> Attrs) : Node
{
    /// <inheritdoc/>
    public override string Type => "field";
}
