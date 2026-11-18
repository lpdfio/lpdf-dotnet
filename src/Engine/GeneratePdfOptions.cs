namespace Lpdf.Engine;

/// <summary>The options of <see cref="LCrossIndustryInvoice.GeneratePdf(PdfEngine, string, GeneratePdfOptions)"/>.</summary>
public sealed class GeneratePdfOptions
{
    /// <summary>The invoice: its XML, which the values are read from and which the PDF carries byte for byte.</summary>
    public required byte[] InvoiceXml { get; init; }

    /// <summary>
    /// The map, an XML document: without it, Lpdf's default map (<see cref="LCrossIndustryInvoice.DefaultInvoiceMap"/>).
    /// A map given takes over whole. A document built in code has no bindings and needs none.
    /// </summary>
    public string? InvoiceMap { get; init; }

    /// <summary>How the values of each type are written.</summary>
    public InvoiceFormat? Format { get; init; }

    /// <summary>The values the XML lacks, which a template binds without a prefix, as <c>note</c>.</summary>
    public object? Data { get; init; }

    /// <summary>The ISO 8601 creation date of the file, which PDF/A requires.</summary>
    public required string CreatedOn { get; init; }
}
