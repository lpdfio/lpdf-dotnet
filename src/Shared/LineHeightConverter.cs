using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Lpdf.Shared;

/// <summary>Writes a <see cref="LineHeight"/> as a string for a name and a number for a factor, and reads either.</summary>
internal sealed class LineHeightConverter : JsonConverter<LineHeight>
{
    public override LineHeight Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType switch
        {
            // A String token always reads as a string: GetString returns null for a Null token only.
            JsonTokenType.String => reader.GetString()!,
            JsonTokenType.Number => reader.GetDouble(),
            var other => throw new JsonException($"a line-height is a string or a number, not {other}"),
        };

    public override void Write(Utf8JsonWriter writer, LineHeight value, JsonSerializerOptions options)
    {
        if (value.Name is not null) writer.WriteStringValue(value.Name);
        // JSON has no NaN or infinity: such a factor goes as its text, which the engine refuses.
        else if (!double.IsFinite(value.Factor)) writer.WriteStringValue(value.Factor.ToString(CultureInfo.InvariantCulture));
        else writer.WriteNumberValue(value.Factor);
    }
}
