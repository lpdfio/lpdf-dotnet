using Lpdf.Layout;

namespace Lpdf.Kit;

/// <summary>
/// Abstract base for content that can appear directly inside a <see cref="SectionNode"/>.
/// Use <see cref="Pdf.Layout"/> or <see cref="Pdf.Canvas"/>
/// to create instances.
/// </summary>
public abstract record SectionContent
{
    /// <summary>The element type name emitted to the wire format.</summary>
    public abstract string Type { get; }
}
