namespace Lpdf.Layout;

/// <summary>
/// A category of a chart: a value for each series, and its label. Produced from <see cref="LChart.Category"/>.
/// </summary>
public sealed record ChartCategoryNode(
    Dictionary<string, object> Attrs,
    List<ChartValueNode>       Values) : ChartPart
{
    /// <inheritdoc/>
    public override string Type => "category";
}
