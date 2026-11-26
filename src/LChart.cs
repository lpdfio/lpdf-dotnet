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
    /// <summary>Build a series of a chart: its entry in the legend and the color of its bars, line or area.</summary>
    public static ChartSeriesNode Series(ChartSeriesAttr? attrs = null)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a category of a chart: a value for each series, in their order, and its label.</summary>
    public static ChartCategoryNode Category(ChartCategoryAttr? attrs = null, ChartValueNode[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>
    /// Build a value of a category: a number, written in full with no culture and no exponent, or a string that is
    /// one, taken as written. No value, an empty list, draws no bar and breaks a line, an area or a sparkline; a
    /// stacked area counts it as zero. It has no attributes: pass <see cref="L.NoAttr"/>.
    /// </summary>
    /// <exception cref="ArgumentException">A value is <c>default(ChartValue)</c>, which holds no number.</exception>
    public static ChartValueNode Value(NoAttributes? attrs = null, ChartValue[]? nodes = null)
    {
        nodes ??= [];
        for (var i = 0; i < nodes.Length; i++)
        {
            if (!nodes[i].IsSet)
            {
                throw new ArgumentException(
                    $"value {i + 1} of <value> is default(ChartValue), which holds no number: give a number or a string",
                    nameof(nodes));
            }
        }
        return new(nodes.Select(value => value.Text).ToList());
    }
}
