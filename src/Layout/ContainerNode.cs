namespace Lpdf.Layout;

/// <summary>A serialised layout container node (column, row, grid, table, etc.).</summary>
public sealed record ContainerNode(
    Dictionary<string, object> Attrs,
    List<Node>                 Nodes,
    string                     _type) : Node
{
    /// <inheritdoc/>
    public override string Type => _type;
}
