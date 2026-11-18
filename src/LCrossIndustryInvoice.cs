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
    /// <summary>The engine's answer to a read, as the errors about its shape name it.</summary>
    private const string ExtractAnswer = "WASM extract_invoice_data response";

    /// <summary>An entry of the answer's <c>typed</c>, as the errors about its fields name it.</summary>
    private const string TypedEntry = ExtractAnswer + ": an entry of 'typed'";

    /// <summary>
    /// The engine that reads invoices for the calls that take no engine of the caller's, <c>ExtractInvoiceData</c> and
    /// <c>DefaultInvoiceMap</c>: it uses no fonts and no key, under the default time and memory limits.
    /// </summary>
    private static readonly Lazy<WasmRunner> Reader =
        new(() => new WasmRunner(EngineOptions.DefaultTimeout, EngineOptions.DefaultMaxMemoryMb));

    /// <summary>
    /// The invoice's PDF, a Factur-X / ZUGFeRD file: <paramref name="template"/>, an XML string, filled with the values
    /// of the invoice, which it binds under the map's prefix (<c>invoice:seller.name</c>) beside
    /// <see cref="GeneratePdfOptions.Data"/>. The file is PDF/A-3, 3b unless the template sets 3a or 3u, and carries the
    /// XML as <c>factur-x.xml</c>, or <c>xrechnung.xml</c> for XRECHNUNG. The template says only how the invoice looks: it
    /// declares no invoice file. The invoice is read and the template rendered on <paramref name="engine"/>, each under
    /// its <see cref="EngineOptions.Timeout"/> and <see cref="EngineOptions.MaxMemoryMb"/>.
    /// </summary>
    /// <param name="engine">The engine, which carries the fonts, the license key and the limits.</param>
    /// <param name="template">The template: how the invoice looks, as lpdf XML.</param>
    /// <param name="options">The invoice's XML, and the map, the format functions, the data and the creation date.</param>
    /// <exception cref="EngineException">
    /// The engine refuses the map or the invoice, as <see cref="ExtractInvoiceData(byte[], ExtractInvoiceDataOptions?)"/>
    /// says, or a template path under the prefix that the map lacks, a PDF/A level other than 3a, 3b or 3u,
    /// <c>encrypt</c>, <c>pdfx</c>, or a template that declares the invoice file; or the read or the render ran past the
    /// engine's timeout or needed more than its memory cap.
    /// </exception>
    /// <exception cref="InvalidOperationException">A format function returned <see langword="null"/>.</exception>
    public static byte[] GeneratePdf(PdfEngine engine, string template, GeneratePdfOptions options)
    {
        ArgumentNullException.ThrowIfNull(engine);
        ArgumentNullException.ThrowIfNull(template);
        Check(options);
        var values = Values(engine.ExtractInvoiceData(options.InvoiceXml, options.InvoiceMap), options.Format);
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
    /// BASIC WL, or lacks a value the map marks required; or the read runs past the default timeout or memory cap,
    /// which it reads under.
    /// </exception>
    /// <exception cref="InvalidOperationException">A format function returned <see langword="null"/>.</exception>
    public static JsonObject ExtractInvoiceData(byte[] invoiceXml, ExtractInvoiceDataOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invoiceXml);
        return Values(Reader.Value.ExtractInvoiceData(invoiceXml, options?.InvoiceMap), options?.Format);
    }

    /// <inheritdoc cref="ExtractInvoiceData(byte[], ExtractInvoiceDataOptions?)"/>
    public static JsonObject ExtractInvoiceData(string invoiceXml, ExtractInvoiceDataOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(invoiceXml);
        return ExtractInvoiceData(Encoding.UTF8.GetBytes(invoiceXml), options);
    }

    /// <summary>
    /// Lpdf's default invoice map, as XML text: the values EN 16931 numbers but a party's identifiers, what a line may
    /// hold several of, and an attached file's content. The start of a map of your own.
    /// </summary>
    public static string DefaultInvoiceMap() => Reader.Value.DefaultInvoiceMap();

    private static void Check(GeneratePdfOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (options.InvoiceXml is null) throw new ArgumentException("GeneratePdf needs InvoiceXml, the invoice's XML.", nameof(options));
        if (options.CreatedOn is null) throw new ArgumentException("GeneratePdf needs CreatedOn, which PDF/A requires.", nameof(options));
    }

    /// <summary>
    /// The values of an invoice, from the engine's answer to <c>extract_invoice_data</c>, with the format function of
    /// each typed one applied.
    /// </summary>
    private static JsonObject Values(string extracted, InvoiceFormat? format)
    {
        var answer = JsonNode.Parse(extracted) as JsonObject
            ?? throw new InvalidOperationException($"{ExtractAnswer} is not a JSON object.");
        var data = Field(answer, "data", ExtractAnswer).AsObject();
        answer.Remove("data");
        foreach (var typed in Field(answer, "typed", ExtractAnswer).AsArray())
        {
            var entry = typed as JsonObject ?? throw new InvalidOperationException($"{TypedEntry} is not a JSON object.");
            var (fn, fnName) = Field(entry, "type", TypedEntry).GetValue<string>() switch
            {
                "date" => (format?.Date, nameof(InvoiceFormat.Date)),
                "amount" => (format?.Amount, nameof(InvoiceFormat.Amount)),
                "code" => (format?.Code, nameof(InvoiceFormat.Code)),
                _ => (null, null),
            };
            if (fn is null) continue;
            var name = Field(entry, "name", TypedEntry).GetValue<string>();
            var at = Field(entry, "at", TypedEntry).AsArray();
            JsonNode holder = data;
            for (var i = 0; i < at.Count - 1; i++) holder = Step(holder, Place(at, i, name), name);
            var last = Place(at, at.Count - 1, name);
            var value = Step(holder, last, name).GetValue<string>();
            // The functions are typed to return text, but a caller's code can still return null, which would bind
            // nothing where the invoice has a value.
            var written = JsonValue.Create(fn(value, name)) ?? throw new InvalidOperationException(
                $"Format.{fnName} returned null for '{name}': return the text to bind, or the value as it came.");
            if (last.GetValueKind() == JsonValueKind.Number) holder.AsArray()[last.GetValue<int>()] = written;
            else holder.AsObject()[last.GetValue<string>()] = written;
        }
        return data;
    }

    /// <summary>The field <paramref name="name"/> of <paramref name="node"/>, a part of the engine's answer named <paramref name="what"/>.</summary>
    private static JsonNode Field(JsonObject node, string name, string what)
        => node[name] ?? throw new InvalidOperationException($"{what} has no '{name}' field.");

    /// <summary>Step <paramref name="index"/> of <paramref name="at"/>, the place of the typed value <paramref name="name"/> in the data.</summary>
    private static JsonNode Place(JsonArray at, int index, string name)
        => index >= 0 && at[index] is { } step
            ? step
            : throw new InvalidOperationException($"{ExtractAnswer}: the 'at' field of '{name}' in 'typed' has no step {index}.");

    /// <summary>
    /// The node at <paramref name="step"/> of <paramref name="holder"/>, a key of an object or an index of an array, on
    /// the way to the typed value <paramref name="name"/>.
    /// </summary>
    private static JsonNode Step(JsonNode holder, JsonNode step, string name)
    {
        var found = step.GetValueKind() == JsonValueKind.Number
            ? holder.AsArray()[step.GetValue<int>()]
            : holder.AsObject()[step.GetValue<string>()];
        return found ?? throw new InvalidOperationException($"{ExtractAnswer}: the 'data' field has no value where 'typed' places '{name}'.");
    }
}
