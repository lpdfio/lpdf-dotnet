namespace Lpdf.Markdown;

/// <summary>
/// The rules that say how the parts of a markdown look, with a file of them in <c>Src</c> when it has one.
/// Produced from <see cref="LMarkdown.Styles"/>.
/// </summary>
public sealed record MarkdownStylesNode(
    Dictionary<string, string> Attrs,
    List<MarkdownRule>         Rules) : MarkdownPart
{
    /// <inheritdoc/>
    public override string Type => "markdown-styles";
}
