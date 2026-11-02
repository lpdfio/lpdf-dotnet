namespace Lpdf.Engine;

/// <summary>
/// Construction-time configuration for <see cref="PdfEngine"/>.
/// Passed once at construction; applies to every subsequent render call.
/// </summary>
public sealed class EngineOptions
{
    /// <summary>The seconds a render may run when <see cref="Timeout"/> is left out.</summary>
    internal const int DefaultTimeout = 5;

    private readonly int _timeout = DefaultTimeout;

    /// <summary>
    /// The seconds a render may run before it stops with an <see cref="EngineException"/> that names the limit:
    /// 5 when left out, no limit at 0. A render stopped at its limit leaves the engine as it was, so the next render
    /// on it runs as usual.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The value is negative.</exception>
    public int Timeout
    {
        get => _timeout;
        init
        {
            ArgumentOutOfRangeException.ThrowIfNegative(value);
            _timeout = value;
        }
    }

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
