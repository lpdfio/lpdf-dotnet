namespace Lpdf.Layout;

/// <summary>
/// What a chart holds: its series and its categories. Use <see cref="LChart.Series"/> and
/// <see cref="LChart.Category"/> to create them.
/// </summary>
public abstract record ChartPart
{
    /// <summary>The element type name emitted to the wire format.</summary>
    public abstract string Type { get; }
}
