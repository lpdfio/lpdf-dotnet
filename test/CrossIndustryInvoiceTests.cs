using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Layout;
using Xunit;

namespace Lpdf.Tests;

/// <summary>
/// LCrossIndustryInvoice: a Factur-X / ZUGFeRD invoice PDF made from the invoice's XML. The template, the invoices, the
/// font and the snapshot hash are core's, its test/fixtures, test/assets and test/snapshots, which CI copies in from the
/// core tag.
/// </summary>
public class CrossIndustryInvoiceTests
{
    private const string CreatedOn = "2026-11-18T10:00:00Z";
    private const string Note = "Payable within 30 days, by bank transfer.";
    private static readonly string Facturx = Path.Combine(SnapshotHelper.Root, "test", "assets", "files", "facturx");

    private static byte[] Invoice(string name) => File.ReadAllBytes(Path.Combine(Facturx, name));

    private static string Template() =>
        File.ReadAllText(Path.Combine(SnapshotHelper.Fixtures, "showcase-cross-industry-invoice.xml"));

    /// <summary>An unlicensed engine with the template's font loaded under its name.</summary>
    private static PdfEngine Engine() =>
        L.Engine().LoadFont("body", File.ReadAllBytes(Path.Combine(SnapshotHelper.Root, "test", "assets", "fonts", "SourceSans3-Regular.otf")));

    /// <summary>How values are written on the page, as the examples write them: the language's own formatters, and the words for the codes.</summary>
    private static readonly InvoiceFormat Format = new()
    {
        Date = (value, _) => DateTime.Parse(value, CultureInfo.InvariantCulture).ToString("d MMMM yyyy", CultureInfo.GetCultureInfo("en-GB")),
        Amount = (value, _) => decimal.Parse(value, CultureInfo.InvariantCulture).ToString("N2", CultureInfo.GetCultureInfo("en-GB")),
        Code = (value, entryName) => (entryName, value) switch
        {
            ("typeCode", "380") => "Invoice",
            ("typeCode", "381") => "Credit note",
            ("lines.unit", "H87") => "pcs",
            ("lines.unit", "KGM") => "kg",
            _ => value,
        },
    };

    private static string S(JsonNode? node) => node!.GetValue<string>();

    /// <summary>The page of the template, built in code with the values of <c>ExtractInvoiceData</c>, as the second example builds it.</summary>
    private static PdfDocument DocumentInCode(JsonObject invoiceData)
    {
        var muted = new TextAttr { FontSize = "9pt", Color = "#5b6475" };
        var mutedRight = new TextAttr { FontSize = "9pt", Color = "#5b6475", Align = "right" };
        var right = new TextAttr { Align = "right" };
        var title = new TextAttr { FontSize = "16pt", Color = "#1763cf" };
        var total = new TextAttr { FontSize = "14pt" };
        var seller = invoiceData["seller"]!;
        var buyer = invoiceData["buyer"]!;
        var totals = invoiceData["totals"]!;
        var rows = new List<Node>
        {
            L.Row(new() { Gap = "xs" }, [L.Text(null, ["VAT"]), L.Text(null, [S(totals["vat"])]), L.Text(null, [S(totals["currency"])])]),
        };
        if (totals["vatInTaxCurrency"] is { } inTaxCurrency)
            rows.Add(L.Row(new() { Gap = "xs" }, [L.Text(muted, ["VAT in"]), L.Text(muted, [S(totals["taxCurrency"])]), L.Text(muted, [S(inTaxCurrency)])]));
        rows.Add(L.Row(new() { Gap = "xs" }, [L.Text(total, ["Total"]), L.Text(total, [S(totals["gross"])]), L.Text(total, [S(totals["currency"])])]));

        return L.Document(new DocumentAttr(
            Size: "a4", Font: "body",
            Assets: new DocumentAssets(Fonts: [new FontAttr { Name = "body", Src = "../assets/fonts/SourceSans3-Regular.otf" }]),
            Meta: new DocumentMeta(Title: "Invoice", Author: "Northwind Paper SAS", Lang: "en")),
        [
            L.Section(null, [
                L.Layout(new() { Margin = "48pt" }, [
                    L.Column(new() { Gap = "l" }, [
                        L.Row(new() { Align = "end" }, [
                            L.Column(new() { Width = "fill", Gap = "xs" }, [
                                L.Text(new() { FontSize = "20pt" }, [S(seller["name"])]),
                                L.Text(muted, [S(seller["street"])]),
                                L.Row(new() { Gap = "xs" }, [L.Text(muted, [S(seller["postcode"])]), L.Text(muted, [S(seller["city"])])]),
                                L.Row(new() { Gap = "xs" }, [L.Text(muted, ["VAT"]), L.Text(muted, [S(seller["vatId"])])]),
                            ]),
                            L.Row(new() { Gap = "xs" }, [L.Text(title, [S(invoiceData["typeCode"])]), L.Text(title, [S(invoiceData["number"])])]),
                        ]),
                        L.Divider(new() { Color = "#dde3ed", Thickness = "0.5pt" }),
                        L.Row(new() { Gap = "l" }, [
                            L.Column(new() { Width = "fill", Gap = "xs" }, [
                                L.Text(muted, ["Billed to"]),
                                L.Text(null, [S(buyer["name"])]),
                                L.Text(null, [S(buyer["street"])]),
                                L.Row(new() { Gap = "xs" }, [L.Text(null, [S(buyer["postcode"])]), L.Text(null, [S(buyer["city"])])]),
                            ]),
                            L.Column(new() { Width = "fill", Gap = "xs" }, [
                                L.Text(muted, ["Issued · Due"]),
                                L.Text(null, [S(invoiceData["issued"])]),
                                .. invoiceData["paymentTerms"]!.AsArray().Select(term => (Node)L.Row(null, [L.Text(null, [S(term!["due"])])])),
                            ]),
                        ]),
                        L.Table(new() { Cols = "4fr 1fr 1fr 1fr 1fr 1fr" }, [
                            L.Thead(null, [
                                L.Td(null, [L.Text(muted, ["Item"])]),
                                L.Td(null, [L.Text(mutedRight, ["Qty"])]),
                                L.Td(null, [L.Text(mutedRight, ["Unit"])]),
                                L.Td(null, [L.Text(mutedRight, ["Price"])]),
                                L.Td(null, [L.Text(mutedRight, ["VAT %"])]),
                                L.Td(null, [L.Text(mutedRight, ["Amount"])]),
                            ]),
                            .. invoiceData["lines"]!.AsArray().Select(line => (Node)L.Tr(null, [
                                L.Td(null, [L.Text(null, [S(line!["name"])])]),
                                L.Td(null, [L.Text(right, [S(line["quantity"])])]),
                                L.Td(null, [L.Text(right, [S(line["unit"])])]),
                                L.Td(null, [L.Text(right, [S(line["unitPrice"])])]),
                                L.Td(null, [L.Text(right, [S(line["vatRate"])])]),
                                L.Td(null, [L.Text(right, [S(line["net"])])]),
                            ])),
                        ]),
                        L.Divider(new() { Color = "#dde3ed", Thickness = "0.5pt" }),
                        L.Table(new() { Cols = "1fr 1fr 1fr" }, [
                            L.Thead(null, [
                                L.Td(null, [L.Text(muted, ["VAT rate (%)"])]),
                                L.Td(null, [L.Text(mutedRight, ["Basis"])]),
                                L.Td(null, [L.Text(mutedRight, ["VAT"])]),
                            ]),
                            .. invoiceData["taxes"]!.AsArray().Select(tax => (Node)L.Tr(null, [
                                L.Td(null, [L.Text(null, [S(tax!["rate"])])]),
                                L.Td(null, [L.Text(right, [S(tax["basis"])])]),
                                L.Td(null, [L.Text(right, [S(tax["amount"])])]),
                            ])),
                        ]),
                        L.Column(new() { Gap = "xs", Align = "end" }, [.. rows]),
                        L.Text(muted, [Note]),
                    ]),
                ]),
            ]),
        ]);
    }

    [Fact]
    public void Fills_the_template_with_the_invoice_and_gives_the_pdf_core_gives()
    {
        using var engine = Engine();
        var pdf = LCrossIndustryInvoice.GeneratePdf(engine, Template(), new()
        {
            InvoiceXml = Invoice("en16931.xml"), Data = new { note = Note }, CreatedOn = CreatedOn,
        });
        SnapshotHelper.CompareOrUpdate("showcase-cross-industry-invoice", pdf);
        var text = Encoding.Latin1.GetString(pdf);
        Assert.Contains("<pdfaid:part>3</pdfaid:part>", text);
        Assert.Contains("<fx:DocumentFileName>factur-x.xml</fx:DocumentFileName>", text);
    }

    [Fact]
    public void Gives_the_same_page_from_the_template_and_from_the_document_built_in_code()
    {
        var invoiceXml = Invoice("en16931.xml");
        using var engine = Engine();
        var fromTemplate = LCrossIndustryInvoice.GeneratePdf(engine, Template(), new()
        {
            InvoiceXml = invoiceXml, Format = Format, Data = new { note = Note }, CreatedOn = CreatedOn,
        });
        var invoiceData = LCrossIndustryInvoice.ExtractInvoiceData(invoiceXml, new() { Format = Format });
        var fromCode = LCrossIndustryInvoice.GeneratePdf(engine, DocumentInCode(invoiceData), new() { InvoiceXml = invoiceXml, CreatedOn = CreatedOn });
        Assert.Equal(fromTemplate, fromCode);
    }

    [Fact]
    public void Extracts_the_values_core_reads_and_formats_them_by_type_and_entry_name()
    {
        var expected = JsonNode.Parse(File.ReadAllText(Path.Combine(Facturx, "case-vat-rates.json")));
        Assert.True(JsonNode.DeepEquals(expected, LCrossIndustryInvoice.ExtractInvoiceData(Invoice("case-vat-rates.xml"))));
        var formatted = LCrossIndustryInvoice.ExtractInvoiceData(Invoice("en16931.xml"), new() { Format = Format });
        Assert.Equal("31 October 2026", S(formatted["issued"]));
        Assert.Equal("Invoice", S(formatted["typeCode"]));
        Assert.Equal("pcs", S(formatted["lines"]![0]!["unit"]));
        Assert.Equal("69.60", S(formatted["totals"]!["gross"]));
        Assert.Equal("30 November 2026", S(formatted["paymentTerms"]![0]!["due"]));
    }

    [Fact]
    public void Reads_through_a_map_given_under_its_own_names()
    {
        const string map = "<cross-industry-invoice-map prefix=\"rechnung\"><value name=\"nummer\" path=\"rsm:ExchangedDocument/ram:ID\"/></cross-industry-invoice-map>";
        var xml = File.ReadAllText(Path.Combine(Facturx, "en16931.xml"));
        var data = LCrossIndustryInvoice.ExtractInvoiceData(xml, new() { InvoiceMap = map });
        Assert.True(JsonNode.DeepEquals(JsonNode.Parse("{\"nummer\":\"2026-0412\"}"), data));
    }

    [Fact]
    public void Gives_the_default_map_as_xml_text()
    {
        var map = LCrossIndustryInvoice.DefaultInvoiceMap();
        Assert.StartsWith("<?xml", map);
        Assert.Contains("<cross-industry-invoice-map prefix=\"invoice\">", map);
    }

    [Fact]
    public void Refuses_what_the_engine_refuses_with_its_error()
    {
        var error = Assert.Throws<EngineException>(() => LCrossIndustryInvoice.ExtractInvoiceData(Invoice("minimum.xml")));
        Assert.StartsWith("the invoice's profile is MINIMUM", error.Message);
        using var engine = Engine();
        var typo = Template().Replace("invoice:seller.name", "invoice:sellr.name");
        error = Assert.Throws<EngineException>(() =>
            LCrossIndustryInvoice.GeneratePdf(engine, typo, new() { InvoiceXml = Invoice("en16931.xml"), CreatedOn = CreatedOn }));
        Assert.StartsWith("data-value=\"invoice:sellr.name\": 'sellr' is not in the map", error.Message);
    }
}
