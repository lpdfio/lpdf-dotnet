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
        var doc = L.Document();
        Assert.Equal(1,          doc.Version);
        Assert.Equal("document", doc.Type);
    }

    [Fact]
    public void Document_MetaAndTokensInAttrs()
    {
        var doc = L.Document(attrs: new DocumentAttr(
            Meta:   new DocumentMeta(Title: "Test"),
            Tokens: new DocumentTokens(Colors: new() { ["primary"] = "#ff0000" })
        ));

        Assert.True(doc.Attrs.ContainsKey("meta"));
        Assert.True(doc.Attrs.ContainsKey("tokens"));
    }

    [Fact]
    public void Document_LineHeightRowIsATokenRowUnderItsSchemaName()
    {
        var doc = L.Document(attrs: new DocumentAttr(
            Tokens: new DocumentTokens(LineHeight: new() { ["m"] = 1.2, ["xxl"] = 2 })
        ));

        var tokens = Assert.IsType<Dictionary<string, object?>>(doc.Attrs["tokens"]);
        Assert.True(tokens.ContainsKey("line-height"));
        Assert.Equal(2.0, ((Dictionary<string, double>)tokens["line-height"]!)["xxl"]);
    }

    [Fact]
    public void Document_AliasesAreATokenRowOfNamesForSlots()
    {
        var doc = L.Document(attrs: new DocumentAttr(
            Tokens: new DocumentTokens(Aliases: new() { ["caption"] = "xs", ["sub-title"] = "l" })
        ));

        var tokens = Assert.IsType<Dictionary<string, object?>>(doc.Attrs["tokens"]);
        var aliases = Assert.IsType<Dictionary<string, string>>(tokens["aliases"]);
        Assert.Equal("xs", aliases["caption"]);
        Assert.Equal("l", aliases["sub-title"]);
    }

    [Fact]
    public void Document_HeightIsATokenRowUnderItsSchemaName()
    {
        var row = new Dictionary<string, string> { ["xs"] = "10pt", ["m"] = "30pt", ["xxl"] = "60pt" };
        var doc = L.Document(attrs: new DocumentAttr(Tokens: new DocumentTokens(Height: row)));

        var tokens = Assert.IsType<Dictionary<string, object?>>(doc.Attrs["tokens"]);
        Assert.Same(row, tokens["height"]);
    }

    [Fact]
    public void Document_GradientsAreATokenRowOfNamedGradients()
    {
        var doc = L.Document(attrs: new DocumentAttr(
            Tokens: new DocumentTokens(Gradients: new()
            {
                ["fade"] = new Gradient { Type = "linear", From = "primary", To = "#0f3e8a", Angle = 90 },
            })
        ));

        var tokens = Assert.IsType<Dictionary<string, object?>>(doc.Attrs["tokens"]);
        var gradients = Assert.IsType<Dictionary<string, Dictionary<string, object>>>(tokens["gradients"]);
        Assert.Equal("linear", gradients["fade"]["type"]);
        Assert.Equal<object>(90.0, gradients["fade"]["angle"]);
    }

    [Fact]
    public void Text_LineHeightIsATokenOrAFactorUnderItsSchemaName()
    {
        Assert.Equal("l",   L.Text(new TextAttr { LineHeight = "l" },   ["A token"]).Attrs["line-height"]);
        Assert.Equal<object>(1.5, L.Text(new TextAttr { LineHeight = 1.5 }, ["A factor"]).Attrs["line-height"]);
    }

    [Fact]
    public void Document_StringOptionsAppearedInAttrs()
    {
        var doc = L.Document(attrs: new DocumentAttr(Size: "a4", Orientation: "landscape"));
        Assert.Equal("a4",        doc.Attrs["size"]);
        Assert.Equal("landscape", doc.Attrs["orientation"]);
    }

    [Fact]
    public void Layout_MarginAppearsInAttrs()
    {
        var layout = L.Layout(new LayoutAttr { Margin = "20pt 30pt" });
        Assert.Equal("20pt 30pt", layout.Attrs["margin"]);
    }

    // -- Layout helpers --------------------------------------------------------

    [Fact]
    public void Column_ProducesContainerNodeWithType()
    {
        var node      = L.Column(attrs: new ColumnAttr { Gap = "m", Background = "surface" });
        var container = Assert.IsType<ContainerNode>(node);
        Assert.Equal("column",  container.Type);
        Assert.Equal("m",       container.Attrs["gap"]);
        Assert.Equal("surface", container.Attrs["background"]);
    }

    [Fact]
    public void Grid_ColWidthKebabCased()
    {
        var node = (ContainerNode)L.Grid(attrs: new GridAttr { ColWidth = "120pt", Cols = 3 });
        Assert.Equal("120pt", node.Attrs["col-width"]);
        Assert.Equal<object>(3,     node.Attrs["cols"]);
    }

    [Fact]
    public void Text_AcceptsRawStringsAndSpans()
    {
        var raw  = L.Raw("Total: ");
        var span = L.Span(
            nodes: ["$100"],
            attrs: new SpanAttr { Bold = true, Color = "primary" });

        var node = L.Text(nodes: [raw, span]);
        Assert.Equal(2, node.Children.Count);
        Assert.False(node.Children[0] is SpanNode);
        var s = Assert.IsType<SpanNode>(node.Children[1]);
        Assert.Equal<object>(true,    s.Attrs["bold"]);
        Assert.Equal("primary", s.Attrs["color"]);
    }

    [Fact]
    public void Divider_HasNoChildren()
    {
        var node = L.Divider(attrs: new DividerAttr { Color = "surface-alt" });
        Assert.Equal("divider",     node.Type);
        Assert.Equal("surface-alt", node.Attrs["color"]);
    }

    [Fact]
    public void Section_ChildrenAreLayoutNodes()
    {
        var layout  = L.Layout(null, [L.Column()]);
        var section = L.Section(
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
        L.Document(nodes: [L.Section(nodes: [L.Layout(null)])]);

    [Fact]
    public void KitToXml_ReturnsXmlDeclaration()
    {
        var xml = L.ToXml(SimpleDoc());
        Assert.StartsWith("<?xml version=\"1.0\"", xml);
    }

    [Fact]
    public void KitToXml_ContainsLpdfRoot()
    {
        var xml = L.ToXml(SimpleDoc());
        Assert.Contains("<lpdf version=\"1\">", xml);
    }

    [Fact]
    public void KitToXml_WritesTheAssetsUnderTheSchemaNames()
    {
        var doc = L.Document(
            attrs: new DocumentAttr(Assets: new DocumentAssets(
                Fonts:
                [
                    new FontAttr { Name = "heading", Core = "Helvetica-Bold" },
                    new FontAttr { Name = "body", Ref = "body-font", Src = "/fonts/MyFont.ttf" },
                ],
                Images: [new ImageAttr { Name = "logo", Src = "logo.png" }])),
            nodes: [L.Section(nodes: [L.Layout(null)])]);
        var xml = L.ToXml(doc);

        Assert.Contains("<font name=\"heading\" core=\"Helvetica-Bold\"/>", xml);
        Assert.Contains("<font name=\"body\" ref=\"body-font\" src=\"/fonts/MyFont.ttf\"/>", xml);
        Assert.Contains("<image name=\"logo\" src=\"logo.png\"/>", xml);
    }

    [Fact]
    public void KitToXml_WritesTheAliasesOfTheTokens()
    {
        var doc = L.Document(
            attrs: new DocumentAttr(Tokens: new DocumentTokens(Aliases: new() { ["caption"] = "xs", ["sub-title"] = "l" })),
            nodes: [L.Section(nodes: [L.Layout(null, [L.Text(new TextAttr { FontSize = "caption" }, ["Small"])])])]);
        var xml = L.ToXml(doc);

        Assert.Contains("<aliases>", xml);
        Assert.Contains("<alias name=\"caption\" value=\"xs\"/>", xml);
        Assert.Contains("<alias name=\"sub-title\" value=\"l\"/>", xml);
        Assert.Contains("<text font-size=\"caption\">", xml);
    }

    [Fact]
    public void KitToXml_WritesTheHeightRowOfTheTokens()
    {
        var doc = L.Document(
            attrs: new DocumentAttr(Tokens: new DocumentTokens(Height: new() { ["xs"] = "10pt", ["m"] = "30pt" })),
            nodes: [L.Section(nodes: [L.Layout(null, [L.Column(new ColumnAttr { Height = "m" })])])]);
        var xml = L.ToXml(doc);

        Assert.Contains("<height ", xml);
        Assert.Contains("xs=\"10pt\"", xml);
        Assert.Contains("m=\"30pt\"", xml);
        Assert.Contains("<column height=\"m\"", xml);
    }

    [Fact]
    public void KitToXml_WritesTheGradientsOfTheTokens()
    {
        var doc = L.Document(
            attrs: new DocumentAttr(Tokens: new DocumentTokens(Gradients: new()
            {
                ["fade"] = new Gradient { Type = "linear", From = "primary", To = "#0f3e8a", Angle = 90 },
                ["sunset"] = new Gradient
                {
                    Type = "radial",
                    Stops =
                    [
                        new GradientStop { Offset = "0%", Color = "#f97316" },
                        new GradientStop { Offset = "100%", Color = "#8b5cf6" },
                    ],
                },
            })),
            nodes: [L.Section(nodes: [L.Layout(null, [L.Column(new ColumnAttr { Background = "gradient:fade" })])])]);
        var xml = L.ToXml(doc);

        Assert.Contains("<gradient name=\"fade\" type=\"linear\" from=\"primary\" to=\"#0f3e8a\" angle=\"90\"/>", xml);
        Assert.Contains("<gradient name=\"sunset\" type=\"radial\">", xml);
        Assert.Contains("offset=\"0%\"", xml);
        Assert.Contains("<column background=\"gradient:fade\"", xml);
    }

    [Fact]
    public void KitToXml_ProducedXmlRendersToValidPdf()
    {
        var doc = L.Document(nodes: [
            L.Section(nodes: [L.Layout(null, [
                L.Text(nodes: [L.Raw("Hello from KitToXml")]),
            ])]),
        ]);
        var xml = L.ToXml(doc);
        using var engine = L.Engine().SetLicenseKey("test-key");
        var pdf = engine.Render(xml);

        Assert.True(pdf.Length > 100);
        Assert.Equal(0x25, pdf[0]); // '%'
        Assert.Equal(0x50, pdf[1]); // 'P'
        Assert.Equal(0x44, pdf[2]); // 'D'
        Assert.Equal(0x46, pdf[3]); // 'F'
    }
}
