namespace Lpdf.Kit;

/// <summary>
/// How the PDF file is written, as the <c>output</c> element of the XML is: whether it is encrypted, and what a
/// reader may do with it, and the standard it meets. The passwords are not here: they come with the render call, on
/// <see cref="Lpdf.Engine.RenderOptions"/>.
/// </summary>
/// <param name="Encrypt">
/// The cipher the PDF is encrypted with, <see cref="Cipher.Aes256"/> or <see cref="Cipher.Rc4128"/>. Without it
/// the PDF is not encrypted.
/// </param>
/// <param name="Allow">
/// What a reader may do without the owner password, separated by spaces (<see cref="Permission"/>): <c>all</c>, or
/// any of <c>print</c>, <c>modify</c>, <c>copy</c>, <c>annotate</c>, <c>fill-forms</c>, <c>accessibility</c>,
/// <c>assemble</c>. A permission comes only from naming it: without it, or with <c>""</c>, nothing is allowed, and
/// <c>all</c> allows everything. It needs <paramref name="Encrypt"/>.
/// </param>
/// <param name="PdfA">
/// Writes the PDF as PDF/A, the standard for files kept for the long term: <c>"2b"</c>, or <c>"2u"</c> when its text
/// must also be searchable. The render needs <see cref="Lpdf.Engine.RenderOptions.CreatedOn"/>, and every text a
/// font of the assets. What PDF/A cannot hold is an error that says what to change: a built-in font, a CMYK color, a
/// form field, a barcode's digits, <paramref name="Encrypt"/>. It is <c>pdfa</c> in the XML.
/// </param>
/// <param name="PdfUa">
/// Writes the PDF as PDF/UA-1, a tagged PDF a screen reader can read: <c>"1"</c>. It combines with
/// <paramref name="PdfA"/>. The render needs <see cref="Lpdf.Engine.RenderOptions.CreatedOn"/>, the meta a title and a
/// lang, every image and barcode of the layout an alt, and every text a font of the assets. What PDF/UA cannot hold is
/// an error that says what to change. It is <c>pdfua</c> in the XML.
/// </param>
public sealed record DocumentOutput(string? Encrypt = null, string? Allow = null, string? PdfA = null, string? PdfUa = null);
