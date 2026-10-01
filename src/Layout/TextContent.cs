namespace Lpdf.Layout;

/// <summary>
/// One item of a <c>text</c> node: a plain string or a <see cref="SpanNode"/>. Both convert to it, so a
/// list can mix them: <c>L.Text(NoAttr, ["Total: ", L.Span(attrs, ["$100"])])</c>.
/// </summary>
/// <remarks>
/// <see cref="Content"/> is an interface, and C# allows no conversions on an interface, so this struct
/// carries the conversions instead. The default value reads as an empty run.
/// </remarks>
public readonly struct TextContent
{
    private readonly Content? _value;

    private TextContent(Content value) => _value = value;

    internal Content Value => _value ?? new RawText(string.Empty);

    /// <summary>A plain-text run.</summary>
    public static implicit operator TextContent(string text) => new(new RawText(text));

    /// <summary>An inline <c>span</c>.</summary>
    public static implicit operator TextContent(SpanNode span) => new(span);
}
