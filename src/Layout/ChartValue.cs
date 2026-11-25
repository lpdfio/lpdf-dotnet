using System.Globalization;
using System.Text.RegularExpressions;

namespace Lpdf.Layout;

/// <summary>
/// A chart's value as the engine reads it: a number written in full, with no culture and no exponent, or a string that
/// is a number, such as <c>"0.250"</c>, taken as written so its label keeps its decimals. It converts from an
/// <c>int</c>, a <c>long</c>, a <c>double</c>, a <c>decimal</c> and a <c>string</c>, so
/// <c>LChart.Value(NoAttr, [41200])</c> and <c>LChart.Value(NoAttr, ["41200"])</c> both compile. Left to
/// <c>ToString()</c>, <c>0.25</c> is <c>0,25</c> on a machine set to German, which the engine refuses.
/// </summary>
public readonly record struct ChartValue
{
    private ChartValue(string text) => Text = text;

    /// <summary>The value as the document writes it.</summary>
    public string Text { get; }

    /// <summary>A whole number.</summary>
    public static implicit operator ChartValue(int value) => new(value.ToString(CultureInfo.InvariantCulture));

    /// <summary>A whole number.</summary>
    public static implicit operator ChartValue(long value) => new(value.ToString(CultureInfo.InvariantCulture));

    /// <summary>A number, written in full: <c>1E+21</c> is <c>1000000000000000000000</c>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The number is not finite.</exception>
    public static implicit operator ChartValue(double value) => new(Plain(value));

    /// <summary>A number, with the decimals it has: <c>0.250m</c> is <c>0.250</c>.</summary>
    public static implicit operator ChartValue(decimal value) => new(value.ToString(CultureInfo.InvariantCulture));

    /// <summary>A number as text, taken as written, which the engine checks is a plain number.</summary>
    /// <exception cref="ArgumentNullException">The text is <c>null</c>.</exception>
    public static implicit operator ChartValue(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        return new ChartValue(text);
    }

    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>The shortest digits that read back as <paramref name="value"/>, with the exponent written out.</summary>
    internal static string Plain(double value)
    {
        if (!double.IsFinite(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "a chart's value is a finite number");
        }
        var text = value.ToString("R", CultureInfo.InvariantCulture);
        var exponent = Regex.Match(text, @"^(-?)(\d+)(?:\.(\d+))?E([+-]\d+)$");
        if (!exponent.Success)
        {
            return text == "-0" ? "0" : text;
        }
        var sign = exponent.Groups[1].Value;
        var whole = exponent.Groups[2].Value;
        var digits = whole + exponent.Groups[3].Value;
        var point = whole.Length + int.Parse(exponent.Groups[4].Value, CultureInfo.InvariantCulture);
        if (point <= 0)
        {
            return $"{sign}0.{new string('0', -point)}{digits}";
        }
        if (point >= digits.Length)
        {
            return sign + digits + new string('0', point - digits.Length);
        }
        return $"{sign}{digits[..point]}.{digits[point..]}";
    }
}
