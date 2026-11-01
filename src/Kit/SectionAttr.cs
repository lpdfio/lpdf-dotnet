namespace Lpdf.Kit;

/// <summary>
/// Attributes for the <c>section</c> (page) element. <c>Bleed</c> is how far this section's pages reach past their trim
/// edge, such as <c>"5mm"</c>: it overrides the document's, and <c>"0mm"</c> takes it away.
/// </summary>
public sealed record SectionAttr(
    string? Size        = null, string? Orientation = null,
    string? Background  = null, string? Title       = null, bool? Debug = null,
    string? Type        = null, string? TocIndentStep = null, string? FootnoteCounter = null,
    string? Bleed       = null);
