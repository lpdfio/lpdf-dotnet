// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>The values of the <c>type</c> attribute of a chart: the kind of chart. bar draws a bar for each value, line a line through the values of each series, area a line filled down to zero, or stacked, and sparkline a small line with no axis, labels or legend.</summary>
public static class ChartType
{
    public const string Bar = "bar";
    public const string Line = "line";
    public const string Area = "area";
    public const string Sparkline = "sparkline";
}
