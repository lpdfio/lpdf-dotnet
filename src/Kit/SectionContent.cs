using Lpdf.Layout;

namespace Lpdf.Kit;

/// <summary>
/// Abstract base for content that can appear directly inside a <see cref="SectionNode"/>: a bookmark, then
/// layers, then at most one layout, then layers. Use <see cref="L.Bookmark"/>, <see cref="L.Layer"/> and
/// <see cref="L.Layout"/> to create instances.
/// </summary>
public abstract record SectionContent
{
    /// <summary>The element type name emitted to the wire format.</summary>
    public abstract string Type { get; }
}
