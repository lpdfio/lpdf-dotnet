namespace Lpdf.Engine;

/// <summary>
/// Construction-time configuration for <see cref="PdfEngine"/>.
/// Passed once at construction; applies to every subsequent render call.
/// </summary>
public sealed class EngineOptions
{
    /// <summary>
    /// File-read callback for resolving the <c>src</c> paths of fonts and images at render time.
    /// On the server this can be set to <c>System.IO.File.ReadAllBytes</c>.
    /// In sandboxed environments supply all bytes via
    /// <see cref="RenderOptions"/> instead.
    /// </summary>
    public Func<string, byte[]>? SrcFallback { get; init; }
}
