namespace Lpdf.Kit;

/// <summary>PDF document metadata written into the output file.</summary>
/// <param name="Lang">
/// The language the document is written in, as a language tag such as <c>en-US</c>. A PDF/UA document needs it.
/// </param>
public sealed record DocumentMeta(
    string? Title    = null, string? Author   = null, string? Subject  = null,
    string? Keywords = null, string? Creator  = null, string? Lang     = null);
