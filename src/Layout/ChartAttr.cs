// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>A chart drawn from numbers, written like a table: series are its columns, each category a row and each value a cell, a value for each series in the series' order. A chart with one series needs no series. It takes its box in the flow as an image does, never splits across pages, and moves whole to the next page when it doesn't fit. The engine draws its axis, steps, gridlines, labels and legend by fixed rules: the value axis starts at zero and runs in round steps, each 1, 2 or 5 times a power of ten, written in full; a negative value takes the axis below zero, where a zero line is drawn; a label wider than its room wraps at spaces onto three lines at most, and what still doesn't fit ends in an ellipsis, so a chart always renders. A chart is a leaf, with no padding, background or border: a column around it gives them, and a text above it is its title.</summary>
public sealed record ChartAttr
{
    /// <summary>The kind of chart. bar draws a bar for each value, side by side within a category, or stacked with stacked="true".</summary>
    public required string Type { get; init; }

    /// <summary>Height of the whole chart, a length or a name from the height row of the tokens. The axis, the labels and the legend take room inside it and the width, and the bars get the rest. The width is fill unless it is set.</summary>
    public required string Height { get; init; }

    /// <summary>Which way the bars run. vertical, the default, puts the categories along the bottom and the value axis on the left; horizontal lists the categories down the left side, which suits long labels, with the value axis along the bottom.</summary>
    public string? Direction { get; init; }

    /// <summary>Stacks the bars of a category on each other, in the order of the series, so the axis runs to the largest total. A stacked value can't be negative. Default false: side by side.</summary>
    public bool? Stacked { get; init; }

    /// <summary>Width of the element. A length fixes it. fill, in a row, takes the width its line leaves after the other children, shared equally if several on the line use fill, and never moves to the next line; in a column it takes the full width.</summary>
    public string? Width { get; init; }

    /// <summary>Written before every number of the chart, the steps and the value labels: $ writes $41,200.</summary>
    public string? Prefix { get; init; }

    /// <summary>Written after every number of the chart: % writes 12%, and " ms" writes 12 ms. With values in thousands, suffix="k" writes $412k.</summary>
    public string? Suffix { get; init; }

    /// <summary>The decimals of the value labels, 0 to 6. Left out, a value is written with the decimals it has. The steps of the axis take the decimals their size needs.</summary>
    public int? Decimals { get; init; }

    /// <summary>A number the value axis reaches down to at least, which takes it below zero: min="-50". It only widens the axis: a value below it still fits.</summary>
    public double? Min { get; init; }

    /// <summary>A number the value axis reaches at least: max="100" for percentages, or the same max on charts meant to be compared side by side. It only widens the axis: a value above it still fits.</summary>
    public double? Max { get; init; }

    /// <summary>The character between the groups of three digits of a number, or empty for none. Default ,: with decimal-mark="," and thousands-mark=".", 1234.5 is written 1.234,5; with a space, 1 234,5.</summary>
    public string? ThousandsMark { get; init; }

    /// <summary>The character before the decimals of a number. Default ., and it can't be the thousands-mark.</summary>
    public string? DecimalMark { get; init; }

    /// <summary>The space between the bars and the axis labels, and between them and the legend. Default s, from the space row of the tokens.</summary>
    public string? Gap { get; init; }

    /// <summary>The space between the bars of one category, side by side. Default a tenth of a bar.</summary>
    public string? BarGap { get; init; }

    /// <summary>The space between categories. Default a quarter of the room of a category. The bars take the room the two gaps leave.</summary>
    public string? CategoryGap { get; init; }

    /// <summary>Where the legend goes, which lists each series with its label and its color: bottom, top, right, or none. Default bottom with two series or more, and none with one.</summary>
    public string? Legend { get; init; }

    /// <summary>Writes each value at the end of its bar, and the total of a stack at the end of the stack. Default false.</summary>
    public bool? ValueLabels { get; init; }
    public string? Font { get; init; }
    public string? FontSize { get; init; }

    /// <summary>The color of the chart's text, its labels, steps and legend, and of its zero line. Default the color of the text around it.</summary>
    public string? Color { get; init; }

    /// <summary>What the chart shows, which a screen reader reads in a tagged document, one with pdfua or a pdfa of level a: "Revenue per month, highest in March". alt="" marks it as decoration. A PDF/UA document needs it on every chart of the layout; at PDF/A level a without pdfua, a chart without it is decoration. A document that is not tagged ignores it.</summary>
    public string? Alt { get; init; }
}
