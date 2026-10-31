using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

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

/// <summary>Writes a <see cref="LineHeight"/> as a string for a name and a number for a factor, and reads either.</summary>
internal sealed class LineHeightConverter : JsonConverter<LineHeight>
{
    public override LineHeight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString()!,
            JsonTokenType.Number => reader.GetDouble(),
            var other => throw new JsonException($"a line-height is a string or a number, not {other}"),
        };

    public override void Write(Utf8JsonWriter writer, LineHeight value, JsonSerializerOptions options)
    {
        if (value.Name is not null) writer.WriteStringValue(value.Name);
        else writer.WriteNumberValue(value.Factor);
    }
}
