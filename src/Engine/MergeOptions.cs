namespace Lpdf.Engine;

/// <summary>What <see cref="PdfEngine.Merge"/> does beyond its defaults. Each option is off by default.</summary>
public sealed record MergeOptions
{
    /// <summary>
    /// Merge a file that is signed. A signature covers the bytes of its own file, so it is lost in the merged file, and
    /// what it looked like still prints. Without it, a signed file is an error that names the file.
    /// </summary>
    public bool AllowSigned { get; init; }

    /// <summary>
    /// Merge a file whose owner password restricts printing or copying, which opens without a password. Without it,
    /// such a file is an error that names the file.
    /// </summary>
    public bool AllowRestricted { get; init; }

    /// <summary>
    /// Add a blank side after a document that has an odd number of pages, the size of its last page, so that each
    /// document starts on a front side when the result is printed on both sides of the sheet. None is added after the
    /// last document.
    /// </summary>
    public bool DoubleSided { get; init; }
}
