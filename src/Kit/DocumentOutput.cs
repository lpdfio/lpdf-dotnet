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
/// must also be searchable, or <c>"2a"</c> when it must also be tagged, and <c>"3a"</c>, <c>"3b"</c> or <c>"3u"</c> to
/// carry attachments of any kind. The render needs <see cref="Lpdf.Engine.RenderOptions.CreatedOn"/>, and every text a
/// font of the assets. Level a tags the file as <paramref name="PdfUa"/> does, without PDF/UA's checks: add
/// <paramref name="PdfUa"/> for a file a screen reader can use. What PDF/A cannot hold is an error that says what to
/// change: a built-in font, a CMYK color, a form field, a barcode's digits, <paramref name="Encrypt"/>, attachments in
/// part 2, and in level a a lang that is not a language tag and a character of Unicode's private use area. With
/// <c>"3a"</c>, <c>"3b"</c> or <c>"3u"</c>, an attachment named <c>factur-x.xml</c>, or <c>xrechnung.xml</c> for the
/// XRECHNUNG profile, makes the file a Factur-X or ZUGFeRD e-invoice: the engine reads the profile from BT-24 of that
/// XML and names the invoice in the file's metadata. The XML is attached as given, and its content is not checked. It
/// is <c>pdfa</c> in the XML.
/// </param>
/// <param name="PdfUa">
/// Writes the PDF as PDF/UA-1, a tagged PDF a screen reader can read: <c>"1"</c>. It combines with
/// <paramref name="PdfA"/>. The render needs <see cref="Lpdf.Engine.RenderOptions.CreatedOn"/>, the meta a title and a
/// lang, every image and barcode of the layout an alt, and every text a font of the assets. What PDF/UA cannot hold is
/// an error that says what to change. It is <c>pdfua</c> in the XML.
/// </param>
/// <param name="PdfX">
/// Writes the PDF as PDF/X-4, for a commercial printer: <c>"4"</c>. It needs <paramref name="Profile"/>, and combines
/// with <paramref name="PdfUa"/>. Every page gets a TrimBox and a BleedBox, RGB is tied to sRGB, and links print as
/// text, as PDF/X allows nothing clickable. The render needs <see cref="Lpdf.Engine.RenderOptions.CreatedOn"/>, the meta
/// a title, and every text a font of the assets. What PDF/X cannot hold is an error that says what to change: a profile
/// that is not a printer profile, a built-in font, a form field, a CMYK color with a profile that is not a CMYK one,
/// <paramref name="Encrypt"/>, <paramref name="PdfA"/>, attachments. It is <c>pdfx</c> in the XML.
/// </param>
/// <param name="Profile">
/// The profile of the assets that a PDF/X file is made for: the ICC profile of the printing press. It needs
/// <paramref name="PdfX"/>.
/// </param>
/// <param name="Linearize">
/// Writes the PDF linearized, for fast web view: page 1 comes first in the file, with hints that say where every other
/// page is, so a viewer that fetches the file by byte ranges, as a browser does, shows page 1 while the rest downloads.
/// Each page then names only the fonts and images it draws. With <paramref name="Encrypt"/> the file is encrypted
/// first and linearized last. Any change to the file afterwards, such as a stamp, ends the linearization.
/// </param>
/// <param name="FacturXStrict">
/// Refuses, in a Factur-X e-invoice, the attachments the Factur-X spec does not allow: an invoice file whose mime is
/// not <c>text/xml</c>, an invoice file whose relationship its profile does not allow, and another file with no mime or
/// one that is not of a format the spec lists (PDF, TXT, GIF, TIFF, JPG, CSV, XML, JSON, XLS, XLSX, ODS). Without it,
/// every file is written as the document gives it. It does nothing in a document without <c>factur-x.xml</c> or
/// <c>xrechnung.xml</c>. It is <c>facturx-strict</c> in the XML.
/// </param>
public sealed record DocumentOutput(
    string? Encrypt = null, string? Allow = null, string? PdfA = null, string? PdfUa = null,
    string? PdfX = null, string? Profile = null, bool? Linearize = null, bool? FacturXStrict = null);
