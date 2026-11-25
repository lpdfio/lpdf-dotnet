using Lpdf.Layout;
using Lpdf.Shared;

namespace Lpdf;

/// <summary>
/// The parts of a chart: its series, its categories and their values. The chart itself is <see cref="L.Chart"/>.
///
/// <example>
/// <code>
/// L.Chart(new() { Type = ChartType.Bar, Height = "160pt", Prefix = "$" }, [
///     LChart.Series(new() { Label = "Revenue" }),
///     LChart.Category(new() { Label = "Q1" }, [LChart.Value(NoAttr, [120000])]),
/// ])
/// </code>
/// </example>
/// </summary>
public static class LChart
{
    /// <summary>Build a series of a chart: its entry in the legend and the color of its bars.</summary>
    public static ChartSeriesNode Series(ChartSeriesAttr? attrs = null)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a category of a chart: a value for each series, in their order, and its label.</summary>
    public static ChartCategoryNode Category(ChartCategoryAttr? attrs = null, ChartValueNode[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>
    /// Build a value of a category: a number, written in full with no culture and no exponent, or a string that is
    /// one, taken as written. No value, an empty list, draws no bar. It has no attributes: pass
    /// <see cref="L.NoAttr"/>.
    /// </summary>
    public static ChartValueNode Value(NoAttributes? attrs = null, ChartValue[]? nodes = null)
        => new((nodes ?? []).Select(value => value.Text).ToList());
}
