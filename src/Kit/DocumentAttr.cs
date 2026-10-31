namespace Lpdf.Kit;

/// <summary>
/// Document-level attributes applied as defaults to every section. <c>Attachments</c> are the files the PDF carries
/// inside it, as the <c>attachments</c> element of the XML is: a reader lists them in its attachments panel. Each
/// file's bytes are read from its <c>Src</c> through the engine's <c>SrcFallback</c>, or loaded with <c>LoadFile</c>
/// under its <c>Ref</c> or name.
/// </summary>
public sealed record DocumentAttr(
    string?        Size        = null, string?        Orientation = null,
    string?        Background  = null, string?        Font        = null,
    bool?          Debug       = null,
    DocumentTokens? Tokens     = null, DocumentMeta?  Meta        = null,
    DocumentAssets? Assets     = null, string? FootnoteCounter = null,
    DocumentOutput? Output     = null,
    IReadOnlyList<FileAttr>? Attachments = null);
