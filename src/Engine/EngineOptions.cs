namespace Lpdf.Engine;

/// <summary>
/// Construction-time configuration for <see cref="PdfEngine"/>.
/// Passed once at construction; applies to every subsequent render call.
/// </summary>
public sealed class EngineOptions
{
    /// <summary>
    /// File-read callback for resolving the <c>src</c> paths of fonts, images, color profiles and the files a document
    /// carries in its <c>attachments</c> at render time. On the server this can be set to
    /// <c>System.IO.File.ReadAllBytes</c>.
    /// In sandboxed environments supply all bytes via <see cref="PdfEngine.LoadFile"/> and its kin instead.
    /// </summary>
    /// <remarks>
    /// An attached file's bytes go into the PDF as they are, so with <c>File.ReadAllBytes</c> a template can embed
    /// any file the process can read. Give a template from a source you don't trust a fallback that reads only the
    /// folder its files are in, or none.
    /// </remarks>
    public Func<string, byte[]>? SrcFallback { get; init; }
}
