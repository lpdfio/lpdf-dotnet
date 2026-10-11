namespace Lpdf.Kit;

/// <summary>
/// A <c>bookmark</c> directly inside a <see cref="SectionNode"/>, which marks the section's first page.
/// Produced from <see cref="L.Bookmark"/>.
/// </summary>
public sealed record SectionBookmark(
    Dictionary<string, string> Attrs) : SectionContent
{
    /// <inheritdoc/>
    public override string Type => "bookmark";
}
