namespace Lpdf.Kit;

/// <summary>
/// Attributes for the <c>section</c> (page) element. <c>Bleed</c> is how far this section's pages reach past their trim
/// edge, such as <c>"5mm"</c>: it overrides the document's, and <c>"0mm"</c> takes it away.
/// <c>PageStart</c> is the number of the section's first page, which starts a new run of pages: <c>1</c> restarts
/// the count. <c>PageFormat</c> is how its page numbers are written, as a list's <c>Format</c> is, such as
/// <c>"lower-roman"</c>, and <c>PagePrefix</c> the text before them, such as <c>"A-"</c>. A section that sets none of the
/// three carries on the count, the format and the prefix of the one before. <c>{page}</c> prints a page's number, and
/// <c>{pages}</c> how many pages its run has.
/// <c>Columns</c> sets the section's flow in that many columns, 1 to 12, each filled like a small page, left to right;
/// <c>ColumnGap</c> is the space between two, a space token or a length (<c>"l"</c> by default), <c>ColumnRule</c> a line
/// in the middle of each gap, as a border, such as <c>"0.5pt #ccc"</c>, and <c>ColumnFill</c> how the last page's
/// columns fill: <c>ColumnFill.Auto</c>, in order, the default, or <c>ColumnFill.Balance</c>, levelled.
/// <c>Ruling</c> names a ruling of the tokens, whose lines are drawn on every page of the section, under everything else.
/// </summary>
public sealed record SectionAttr(
    string? Size        = null, string? Orientation = null,
    string? Background  = null, string? Title       = null, bool? Debug = null,
    string? Type        = null, string? TocIndentStep = null, string? FootnoteCounter = null,
    string? Bleed       = null,
    int?    PageStart   = null, string? PageFormat  = null, string? PagePrefix = null,
    int?    Columns     = null, string? ColumnGap   = null, string? ColumnRule = null, string? ColumnFill = null,
    string? Ruling      = null);
