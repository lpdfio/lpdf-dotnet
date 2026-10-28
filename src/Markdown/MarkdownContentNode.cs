namespace Lpdf.Markdown;

/// <summary>The Markdown itself, as strings that make one text. Produced from <see cref="LMarkdown.Content"/>.</summary>
public sealed record MarkdownContentNode(
    List<string> Text) : MarkdownPart
{
    /// <inheritdoc/>
    public override string Type => "markdown-content";
}
