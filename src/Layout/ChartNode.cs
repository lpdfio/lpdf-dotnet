namespace Lpdf.Layout;

/// <summary>A serialised <c>chart</c> node: its attributes, then its series and its categories.</summary>
public sealed record ChartNode(
    Dictionary<string, object> Attrs,
    List<ChartPart>            Parts) : Node
{
    /// <inheritdoc/>
    public override string Type => "chart";
}
