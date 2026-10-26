namespace Lpdf.Markdown;

/// <summary>
/// Abstract base for what a markdown holds: its styles and its content. Use <see cref="LMarkdown.Styles"/> and
/// <see cref="LMarkdown.Content"/> to create instances.
/// </summary>
public abstract record MarkdownPart
{
    /// <summary>The element type name emitted to the wire format.</summary>
    public abstract string Type { get; }
}
