using Lpdf.Markdown;

namespace Lpdf.Kit;

/// <summary>
/// A markdown inside a <see cref="SectionNode"/>, where a layout would go: it holds only Markdown. Produced
/// from <see cref="L.Markdown"/>.
/// </summary>
public sealed record SectionMarkdown(
    List<MarkdownPart>         Nodes,
    Dictionary<string, string> Attrs) : SectionContent
{
    /// <inheritdoc/>
    public override string Type => "markdown";
}
