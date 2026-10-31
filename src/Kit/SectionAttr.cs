namespace Lpdf.Kit;

/// <summary>Attributes for the <c>section</c> (page) element.</summary>
public sealed record SectionAttr(
    string? Size        = null, string? Orientation = null,
    string? Background  = null, string? Title       = null, bool? Debug = null,
    string? Type        = null, string? TocIndentStep = null, string? FootnoteCounter = null);
