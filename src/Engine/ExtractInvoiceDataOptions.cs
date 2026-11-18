namespace Lpdf.Engine;

/// <summary>The options of <see cref="LCrossIndustryInvoice.ExtractInvoiceData(byte[], ExtractInvoiceDataOptions?)"/>.</summary>
public sealed class ExtractInvoiceDataOptions
{
    /// <summary>
    /// The map, an XML document: without it, Lpdf's default map (<see cref="LCrossIndustryInvoice.DefaultInvoiceMap"/>).
    /// A map given takes over whole.
    /// </summary>
    public string? InvoiceMap { get; init; }

    /// <summary>How the values of each type are written.</summary>
    public InvoiceFormat? Format { get; init; }
}
