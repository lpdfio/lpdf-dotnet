namespace Lpdf.Engine;

/// <summary>
/// Per-call options passed to <see cref="PdfEngine.Render(string, RenderOptions?)"/>
/// or <see cref="PdfEngine.Render(Lpdf.Kit.PdfDocument, RenderOptions?)"/>.
/// </summary>
public sealed class RenderOptions
{
    /// <summary>
    /// Optional ISO 8601 creation timestamp (e.g. <c>"2024-06-01T12:00:00"</c>).
    /// When provided, written as <c>/CreationDate</c> in the PDF info dictionary.
    /// Omitting this keeps builds reproducible (no embedded timestamp).
    /// </summary>
    public string? CreatedOn { get; init; }

    /// <summary>
    /// Optional data object for resolving <c>data-*</c> binding attributes in the
    /// XML template. Pass <see langword="null"/> or omit to render with inline
    /// fallback content. Only applies when rendering an XML string.
    /// </summary>
    public object? Data { get; init; }
}
