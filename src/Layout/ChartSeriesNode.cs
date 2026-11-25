namespace Lpdf.Layout;

/// <summary>A series of a chart: its label and color in the legend. Produced from <see cref="LChart.Series"/>.</summary>
public sealed record ChartSeriesNode(
    Dictionary<string, object> Attrs) : ChartPart
{
    /// <inheritdoc/>
    public override string Type => "series";
}
