using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Lpdf.Engine;
using Lpdf.Kit;

namespace Lpdf;

/// <summary>
/// A Factur-X / ZUGFeRD invoice PDF made from the invoice's XML, the EN 16931 Cross Industry Invoice that accounting
/// software writes. The engine reads the XML through a map and checks it; this class hands the values to the caller's
/// <see cref="InvoiceFormat"/> functions.
///
/// <example>
/// <code>
/// var pdf = LCrossIndustryInvoice.GeneratePdf(L.Engine(), template, new()
/// {
///     InvoiceXml = File.ReadAllBytes("invoice.xml"),
///     Format = new() { Date = (value, _) => DateTime.Parse(value).ToString("D", CultureInfo.GetCultureInfo("de-DE")) },
///     CreatedOn = DateTime.UtcNow.ToString("o"),
/// });
/// </code>
/// </example>
/// </summary>
public static class LCrossIndustryInvoice
{
    /// <summary>
    /// The engine that reads invoices with no engine of the caller's: it uses no fonts and no key, under the default
    /// time and memory limits.
    /// </summary>
    private static readonly Lazy<WasmRunner> Reader =
        new(() => new WasmRunner(EngineOptions.DefaultTimeout, EngineOptions.DefaultMaxMemoryMb));

    /// <summary>
    /// The invoice's PDF, a Factur-X / ZUGFeRD file: <paramref name="template"/>, an XML string, filled with the values
    /// of the invoice, which it binds under the map's prefix (<c>invoice:seller.name</c>) beside
    /// <see cref="GeneratePdfOptions.Data"/>. The file is PDF/A-3, 3b unless the template sets 3a or 3u, and carries the
    /// XML as <c>factur-x.xml</c>, or <c>xrechnung.xml</c> for XRECHNUNG. The template says only how the invoice looks: it
    /// declares no invoice file.
    /// </summary>
    /// <param name="engine">The engine, which carries the fonts, the license key and the limits.</param>
    /// <param name="template">The template: how the invoice looks, as lpdf XML.</param>
    /// <param name="options">The invoice's XML, and the map, the format functions, the data and the creation date.</param>
    /// <exception cref="EngineException">
    /// The engine refuses the map or the invoice, as <see cref="ExtractInvoiceData(byte[], ExtractInvoiceDataOptions?)"/>
    /// says, or a template path under the prefix that the map lacks, a PDF/A level other than 3a, 3b or 3u,
    /// <c>encrypt</c>, <c>pdfx</c>, or a template that declares the invoice file.
    /// </exception>
    public static byte[] GeneratePdf(PdfEngine engine, string template, GeneratePdfOptions options)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(template);
        Check(options);
        var values = Read(options.InvoiceXml, options.InvoiceMap, options.Format);
        return engine.RenderInvoice(template, options.InvoiceXml, options.InvoiceMap, values.ToJsonString(), options.Data, options.CreatedOn);
    }

    /// <summary>
    /// The invoice's PDF, a Factur-X / ZUGFeRD file, from a document built in code: the file is PDF/A-3, 3b unless the
    /// document sets 3a or 3u, and carries the XML as <c>factur-x.xml</c>, or <c>xrechnung.xml</c> for XRECHNUNG. The
    /// document has no bindings, so only <see cref="GeneratePdfOptions.InvoiceXml"/> and
    /// <see cref="GeneratePdfOptions.CreatedOn"/> apply.
    /// </summary>
    /// <param name="engine">The engine, which carries the fonts, the license key and the limits.</param>
    /// <param name="document">The document, built with <see cref="L"/>.</param>
    /// <param name="options">The invoice's XML and the creation date.</param>
    /// <exception cref="EngineException">
    /// The engine refuses the invoice, a PDF/A level other than 3a, 3b or 3u, <c>encrypt</c>, <c>pdfx</c>, or a document
    /// that declares the invoice file.
    /// </exception>
    public static byte[] GeneratePdf(PdfEngine engine, PdfDocument document, GeneratePdfOptions options)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(document);
        Check(options);
        return engine.RenderInvoice(document, options.InvoiceXml, options.CreatedOn);
    }

    /// <summary>
    /// The values of an invoice as plain data: a value is a field, a group an object, a list an array, all text, as
    /// the map names them. A value the invoice lacks is absent, and a list with no items is empty. For code that needs
    /// the values itself: a document built in code, choosing a template, naming the file.
    /// </summary>
    /// <param name="invoiceXml">The invoice's XML, an EN 16931 Cross Industry Invoice.</param>
    /// <param name="options">The map, and the format functions, or <see langword="null"/> for the default map as it reads.</param>
    /// <exception cref="EngineException">
    /// The map is not one or doesn't fit the schema, the invoice is not a Cross Industry Invoice or is MINIMUM or
    /// BASIC WL, or lacks a value the map marks required.
    /// </exception>
    public static JsonObject ExtractInvoiceData(byte[] invoiceXml, ExtractInvoiceDataOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invoiceXml);
        return Read(invoiceXml, options?.InvoiceMap, options?.Format);
    }

    /// <inheritdoc cref="ExtractInvoiceData(byte[], ExtractInvoiceDataOptions?)"/>
    public static JsonObject ExtractInvoiceData(string invoiceXml, ExtractInvoiceDataOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invoiceXml);
        return ExtractInvoiceData(Encoding.UTF8.GetBytes(invoiceXml), options);
    }

    /// <summary>Lpdf's default invoice map, as XML text: every value EN 16931 numbers, the start of a map of your own.</summary>
    public static string DefaultInvoiceMap() => Reader.Value.DefaultInvoiceMap();

    private static void Check(GeneratePdfOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.InvoiceXml is null) throw new ArgumentException("GeneratePdf needs InvoiceXml, the invoice's XML.", nameof(options));
        if (options.CreatedOn is null) throw new ArgumentException("GeneratePdf needs CreatedOn, which PDF/A requires.", nameof(options));
    }

    /// <summary>The values of an invoice, read by the engine, with the format function of each typed one applied.</summary>
    private static JsonObject Read(byte[] invoiceXml, string? invoiceMap, InvoiceFormat? format)
    {
        var answer = JsonNode.Parse(Reader.Value.ExtractInvoiceData(invoiceXml, invoiceMap))!.AsObject();
        var data = answer["data"]!.AsObject();
        answer.Remove("data");
        foreach (var typed in answer["typed"]!.AsArray())
        {
            var fn = typed!["type"]!.GetValue<string>() switch
            {
                "date" => format?.Date,
                "amount" => format?.Amount,
                "code" => format?.Code,
                _ => null,
            };
            if (fn is null) continue;
            var at = typed["at"]!.AsArray();
            JsonNode holder = data;
            for (var i = 0; i < at.Count - 1; i++) holder = Step(holder, at[i]!)!;
            var last = at[at.Count - 1]!;
            var value = Step(holder, last)!.GetValue<string>();
            var written = JsonValue.Create(fn(value, typed["name"]!.GetValue<string>()));
            if (last.GetValueKind() == JsonValueKind.Number) holder.AsArray()[last.GetValue<int>()] = written;
            else holder.AsObject()[last.GetValue<string>()] = written;
        }
        return data;
    }

    /// <summary>The node at <paramref name="step"/> of <paramref name="holder"/>: a key of an object or an index of an array.</summary>
    private static JsonNode? Step(JsonNode holder, JsonNode step)
        => step.GetValueKind() == JsonValueKind.Number ? holder.AsArray()[step.GetValue<int>()] : holder.AsObject()[step.GetValue<string>()];
}
