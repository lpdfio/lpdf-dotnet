namespace Lpdf.Kit;

/// <summary>
/// How the PDF file is written, as the <c>output</c> element of the XML is: whether it is encrypted, and what a
/// reader may do with it. The passwords are not here: they come with the render call, on
/// <see cref="Lpdf.Engine.RenderOptions"/>.
/// </summary>
/// <param name="Encrypt">
/// The cipher the PDF is encrypted with, <see cref="Cipher.Aes256"/> or <see cref="Cipher.Rc4128"/>. Without it
/// the PDF is not encrypted.
/// </param>
/// <param name="Allow">
/// What a reader may do without the owner password, separated by spaces (<see cref="Permission"/>): <c>print</c>,
/// <c>modify</c>, <c>copy</c>, <c>annotate</c>, <c>fill-forms</c>, <c>accessibility</c>, <c>assemble</c>. Without
/// it everything is allowed; <c>""</c> allows nothing. It needs <paramref name="Encrypt"/>.
/// </param>
public sealed record DocumentOutput(string? Encrypt = null, string? Allow = null);
