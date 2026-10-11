using Lpdf.Kit;

namespace Lpdf.Layout;

/// <summary>
/// A serialised <c>bookmark</c> node: an entry of the PDF's outline and a target for <c>href="#id"</c> links,
/// which marks the node that follows it. It also goes directly in a section's content, where it marks the
/// section's first page.
/// </summary>
public sealed record BookmarkNode(
    Dictionary<string, string> Attrs) : Node
{
    /// <inheritdoc/>
    public override string Type => "bookmark";

    /// <summary>The same bookmark as a section's own, so <see cref="L.Bookmark"/> also goes in a section's content.</summary>
    public static implicit operator SectionContent(BookmarkNode bookmark) => new SectionBookmark(bookmark.Attrs);
}
