namespace Lpdf.Markdown;

/// <summary>
/// A rule of a markdown's styles: the element it is named after, with the attributes it sets. Produced from
/// <see cref="LMarkdown"/>, as <see cref="LMarkdown.H1"/> or <see cref="LMarkdown.Quote"/>.
/// </summary>
public sealed record MarkdownRule(
    string                     RuleType,
    Dictionary<string, object> Attrs);
