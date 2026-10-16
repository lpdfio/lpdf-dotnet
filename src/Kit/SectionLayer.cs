using Lpdf.Canvas;

namespace Lpdf.Kit;

/// <summary>
/// A <c>layer</c> directly inside a <see cref="SectionNode"/>. Layers before the section's layout paint under it,
/// and layers after it paint over it. Produced from <see cref="L.Layer"/>.
/// </summary>
public sealed record SectionLayer(
    LayerNode Layer) : SectionContent
{
    /// <inheritdoc/>
    public override string Type => "layer";
}
