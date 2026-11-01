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
    /// Required for a document whose <c>output</c> asks for <c>pdfa</c>, <c>pdfua</c> or <c>pdfx</c>, which writes it
    /// as both dates of the file.
    /// </summary>
    public string? CreatedOn { get; init; }

    /// <summary>
    /// Optional data object for resolving <c>data-*</c> binding attributes in the
    /// XML template. Pass <see langword="null"/> or omit to render with inline
    /// fallback content. Only applies when rendering an XML string.
    /// </summary>
    public object? Data { get; init; }

    /// <summary>
    /// The password a reader asks for before showing the PDF, for a document that asks for encryption in its
    /// <c>output</c>. Leave it out, or pass <c>""</c>, for a PDF anyone can open, which only the <c>allow</c> of the
    /// <c>output</c> restricts.
    /// </summary>
    public string? UserPassword { get; init; }

    /// <summary>
    /// The password that opens the PDF with every permission. A document that asks for encryption in its
    /// <c>output</c> renders only with it, or with <see cref="SkipEncryption"/>; a document that doesn't ask for
    /// encryption refuses it, so a file never goes out unencrypted while you believe it is locked.
    /// </summary>
    public string? OwnerPassword { get; init; }

    /// <summary>
    /// Renders a document that asks for encryption unencrypted, for a render that cannot have the passwords, such
    /// as a preview. Not together with the passwords.
    /// </summary>
    public bool SkipEncryption { get; init; }
}
