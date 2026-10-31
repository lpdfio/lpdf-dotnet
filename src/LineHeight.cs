using System.Globalization;
using System.Text.Json.Serialization;
using Lpdf.Shared;

namespace Lpdf;

/// <summary>
/// The distance between the lines of a text: a name from the line-height row of the tokens, such as <c>"m"</c>, or a
/// factor of the font size above 0, such as <c>1.5</c>. It converts from either, so <c>LineHeight = "m"</c> and
/// <c>LineHeight = 1.5</c> both compile, and a document takes the name as a string and the factor as a number.
/// </summary>
[JsonConverter(typeof(LineHeightConverter))]
public readonly record struct LineHeight
{
    private LineHeight(string? name, double factor)
    {
        Name = name;
        Factor = factor;
    }

    /// <summary>The name of a slot or an alias of the line-height row, or <c>null</c> for a factor.</summary>
    public string? Name { get; }

    /// <summary>The factor of the font size, when <see cref="Name"/> is <c>null</c>.</summary>
    public double Factor { get; }

    /// <summary>The value as a document takes it: the name, or the factor as a number.</summary>
    internal object Value => Name is not null ? Name : Factor;

    /// <summary>A name from the line-height row of the tokens, such as <c>"m"</c>.</summary>
    /// <exception cref="ArgumentNullException">The name is <c>null</c>.</exception>
    public static implicit operator LineHeight(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return new LineHeight(name, 0);
    }

    /// <summary>A factor of the font size above 0, such as <c>1.5</c>.</summary>
    public static implicit operator LineHeight(double factor) => new(null, factor);

    /// <inheritdoc/>
    public override string ToString() => Name ?? Factor.ToString(CultureInfo.InvariantCulture);
}
