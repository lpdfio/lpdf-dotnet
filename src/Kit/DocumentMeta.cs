namespace Lpdf.Kit;

/// <summary>PDF document metadata written into the output file.</summary>
/// <param name="Title">The document's title. A PDF/UA document needs it: a reader shows it in place of the file name.</param>
/// <param name="Author">The author of the document.</param>
/// <param name="Subject">What the document is about.</param>
/// <param name="Keywords">Keywords for the document.</param>
/// <param name="Creator">The application that made the document.</param>
/// <param name="Lang">
/// The language the document is written in, as a language tag such as <c>en-US</c>. A PDF/UA document needs it.
/// </param>
public sealed record DocumentMeta(
    string? Title    = null, string? Author   = null, string? Subject  = null,
    string? Keywords = null, string? Creator  = null, string? Lang     = null);
