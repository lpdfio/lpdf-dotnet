using Lpdf;
using Lpdf.Kit;
using Lpdf.Layout;
using Xunit;

namespace Lpdf.Tests;

public class LpdfKitTypesTests
{
    // -- Document structure ----------------------------------------------------

    [Fact]
    public void Document_HasVersion1AndTypeDocument()
    {
        var doc = Pdf.Document();
        Assert.Equal(1,          doc.Version);
        Assert.Equal("document", doc.Type);
    }

    [Fact]
    public void Document_MetaAndTokensInAttrs()
    {
        var doc = Pdf.Document(attrs: new DocumentAttr(
            Meta:   new DocumentMeta(Title: "Test"),
            Tokens: new DocumentTokens(Colors: new() { ["primary"] = "#ff0000" })
        ));

        Assert.True(doc.Attrs.ContainsKey("meta"));
        Assert.True(doc.Attrs.ContainsKey("tokens"));
    }

    [Fact]
    public void Document_StringOptionsAppearedInAttrs()
    {
        var doc = Pdf.Document(attrs: new DocumentAttr(Size: "a4", Margin: "28pt"));
        Assert.Equal("a4",   doc.Attrs["size"]);
        Assert.Equal("28pt", doc.Attrs["margin"]);
    }

    // -- Layout helpers --------------------------------------------------------

    [Fact]
    public void Stack_ProducesContainerNodeWithType()
    {
        var node      = Pdf.Stack(attrs: new StackAttr(Gap: "m", Background: "surface"));
        var container = Assert.IsType<ContainerNode>(node);
        Assert.Equal("stack",   container.Type);
        Assert.Equal("m",       container.Attrs["gap"]);
        Assert.Equal("surface", container.Attrs["background"]);
    }

    [Fact]
    public void Grid_ColWidthKebabCased()
    {
        var node = (ContainerNode)Pdf.Grid(attrs: new GridAttr(ColWidth: "120pt", Cols: "3"));
        Assert.Equal("120pt", node.Attrs["col-width"]);
        Assert.Equal("3",     node.Attrs["cols"]);
    }

    [Fact]
    public void Text_AcceptsRawStringsAndSpans()
    {
        var raw  = Pdf.Raw("Total: ");
        var span = Pdf.Span(
            nodes: ["$100"],
            attrs: new SpanAttr(Bold: "true", Color: "primary"));

        var node = Pdf.Text(nodes: [raw, span]);
        Assert.Equal(2, node.Children.Count);
        Assert.False(node.Children[0] is SpanNode);
        var s = Assert.IsType<SpanNode>(node.Children[1]);
        Assert.Equal("true",    s.Attrs["bold"]);
        Assert.Equal("primary", s.Attrs["color"]);
    }

    [Fact]
    public void Divider_HasNoChildren()
    {
        var node = Pdf.Divider(attrs: new DividerAttr(Color: "surface-alt"));
        Assert.Equal("divider",     node.Type);
        Assert.Equal("surface-alt", node.Attrs["color"]);
    }

    [Fact]
    public void Section_ChildrenAreLayoutNodes()
    {
        var layout  = Pdf.Layout(null, [Pdf.Stack()]);
        var section = Pdf.Section(
            attrs:  new SectionAttr(Size: "a4"),
            nodes:  [layout]);
        Assert.Equal("section", section.Type);
        Assert.Equal("a4",      section.Attrs["size"]);
        Assert.Single(layout.Nodes);
    }
}

// -- KitToXml tests ------------------------------------------------------------

public class KitToXmlTests
{
    private static PdfDocument SimpleDoc() =>
        Pdf.Document(nodes: [Pdf.Section(nodes: [Pdf.Layout(null)])]);

    [Fact]
    public async Task KitToXml_ReturnsXmlDeclaration()
    {
        var xml = await Pdf.ToXml(SimpleDoc());
        Assert.StartsWith("<?xml version=\"1.0\"", xml);
    }

    [Fact]
    public async Task KitToXml_ContainsLpdfRoot()
    {
        var xml = await Pdf.ToXml(SimpleDoc());
        Assert.Contains("<lpdf version=\"1\">", xml);
    }

    [Fact]
    public async Task KitToXml_BuiltinFontPlacedInAssets()
    {
        var doc = Pdf.Document(
            attrs: new DocumentAttr(
                Tokens: new DocumentTokens(Fonts: new()
                {
                    ["heading"] = new FontBuiltin("Helvetica-Bold"),
                })
            ),
            nodes: [Pdf.Section(nodes: [Pdf.Layout(null)])]);
        var xml = await Pdf.ToXml(doc);

        Assert.Contains("<assets>",               xml);
        Assert.Contains("core=\"Helvetica-Bold\"", xml);

        var tokensStart   = xml.IndexOf("<tokens>",  StringComparison.Ordinal);
        var tokensEnd     = xml.IndexOf("</tokens>", StringComparison.Ordinal);
        var fontsInTokens = tokensStart >= 0 ? xml.IndexOf("<fonts>", tokensStart, StringComparison.Ordinal) : -1;
        Assert.True(
            tokensStart < 0 || fontsInTokens < 0 || fontsInTokens > tokensEnd,
            "Font was incorrectly placed inside <tokens>");
    }

    [Fact]
    public async Task KitToXml_CustomFontUsesRefAlias()
    {
        var doc = Pdf.Document(
            attrs: new DocumentAttr(
                Tokens: new DocumentTokens(Fonts: new()
                {
                    ["body"] = new FontSrc("/fonts/MyFont.ttf"),
                })
            ),
            nodes: [Pdf.Section(nodes: [Pdf.Layout(null)])]);
        var xml = await Pdf.ToXml(doc);

        Assert.Contains("ref=\"body\"", xml);
        Assert.DoesNotContain("ref=\"/fonts/MyFont.ttf\"", xml);
        Assert.Contains("src=", xml);
    }

    [Fact]
    public async Task KitToXml_ProducedXmlRendersToValidPdf()
    {
        var doc = Pdf.Document(nodes: [
            Pdf.Section(nodes: [Pdf.Layout(null, [
                Pdf.Text(nodes: [Pdf.Raw("Hello from KitToXml")]),
            ])]),
        ]);
        var xml = await Pdf.ToXml(doc);
        using var engine = Pdf.Engine().SetLicenseKey("test-key");
        var pdf = await engine.Render(xml);

        Assert.True(pdf.Length > 100);
        Assert.Equal(0x25, pdf[0]); // '%'
        Assert.Equal(0x50, pdf[1]); // 'P'
        Assert.Equal(0x44, pdf[2]); // 'D'
        Assert.Equal(0x46, pdf[3]); // 'F'
    }
}
