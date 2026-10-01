using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Lpdf;
using Lpdf.Canvas;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Layout;
using Lpdf.Shared;
using Xunit;

namespace Lpdf.Tests;

/// <summary>The builders write the schema's names, and what they build renders like the XML it converts to.</summary>
public class SchemaTests
{
    // -- Attribute names ---------------------------------------------------------------------------

    [Fact]
    public void Text_align_and_bold_use_the_schema_names()
    {
        var node = L.Text(new TextAttr { Align = "right", Bold = "true" }, ["x"]);

        Assert.Equal(new Dictionary<string, string> { ["align"] = "right", ["bold"] = "true" }, node.Attrs);
    }

    [Fact]
    public void Link_and_span_carry_href()
    {
        Assert.Equal("https://lpdf.io", L.Link(new LinkAttr { Href = "https://lpdf.io" }).Attrs["href"]);
        Assert.Equal("https://lpdf.io", L.Span(new SpanAttr { Href = "https://lpdf.io" }, ["x"]).Attrs["href"]);
    }

    [Fact]
    public void Field_carries_its_type_and_name_as_attributes()
    {
        var node = L.Field(new FieldAttr { Type = FieldType.Text, Name = "email", MaxLen = "40", ActionUrl = "https://lpdf.io" });

        Assert.Equal("text", node.Attrs["type"]);
        Assert.Equal("email", node.Attrs["name"]);
        Assert.Equal("40", node.Attrs["max-len"]);
        Assert.Equal("https://lpdf.io", node.Attrs["action-url"]);
    }

    [Fact]
    public void A_required_attribute_is_a_required_member()
    {
        Assert.NotNull(typeof(LinkAttr).GetProperty("Href")!.GetCustomAttribute<RequiredMemberAttribute>());
        Assert.NotNull(typeof(FieldAttr).GetProperty("Type")!.GetCustomAttribute<RequiredMemberAttribute>());
        Assert.NotNull(typeof(RectAttr).GetProperty("W")!.GetCustomAttribute<RequiredMemberAttribute>());
        Assert.Null(typeof(RectAttr).GetProperty("Fill")!.GetCustomAttribute<RequiredMemberAttribute>());
    }

    [Fact]
    public void Region_is_written_as_region_with_a_schema_pin()
    {
        var node = L.Region(new RegionAttr { Pin = Pin.Top }, [L.Text(null, ["header"])]);

        Assert.Equal("region", node.Type);
        Assert.Equal("top", node.Attrs["pin"]);
    }

    [Fact]
    public void Constants_are_the_values_of_the_schema_enumerations()
    {
        Assert.Equal(new[] { "text", "checkbox", "dropdown", "radio", "button" },
            new[] { FieldType.Text, FieldType.Checkbox, FieldType.Dropdown, FieldType.Radio, FieldType.Button });
        Assert.Equal(new[] { "top", "bottom", "left", "right" }, new[] { Pin.Top, Pin.Bottom, Pin.Left, Pin.Right });
        Assert.Equal(new[] { "portrait", "landscape" }, new[] { Orientation.Portrait, Orientation.Landscape });
        Assert.Equal(new[] { "each", "first", "last", "odd", "even" },
            new[] { PageScope.Each, PageScope.First, PageScope.Last, PageScope.Odd, PageScope.Even });
    }

    [Fact]
    public void Document_font_and_debug_are_written()
    {
        var doc = L.Document(new DocumentAttr(Font: "Times-Roman", Debug: "true"));

        Assert.Equal("Times-Roman", doc.Attrs["font"]);
        Assert.Equal("true", doc.Attrs["debug"]);
    }

    // -- Canvas ------------------------------------------------------------------------------------

    [Fact]
    public void Rect_writes_its_attributes_as_given()
    {
        var node = L.Rect(new RectAttr { X = "10pt", Y = "20pt", W = "100pt", H = "50pt", Fill = "#ff0000", Radius = "5pt" });

        Assert.Equal("rect", node.Type);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["w"] = "100pt", ["h"] = "50pt", ["x"] = "10pt", ["y"] = "20pt", ["fill"] = "#ff0000", ["radius"] = "5pt",
            },
            node.Attrs);
    }

    [Fact]
    public void Canvas_primitives_use_their_schema_names()
    {
        Assert.Equal("line", L.Line(new LineAttr { X1 = "0pt", Y1 = "0pt", X2 = "9pt", Y2 = "9pt", LineCap = "round" }).Type);
        Assert.Equal("circle", L.Circle(new CircleAttr { R = "5pt", Cx = "1pt", Cy = "1pt" }).Type);
        Assert.Equal("ellipse", L.Ellipse(new EllipseAttr { Rx = "5pt", Ry = "3pt" }).Type);
        Assert.Equal("img", L.ImgAt(new CanvasImgAttr { Name = "logo", W = "10pt", H = "10pt" }).Type);

        var path = L.Path(new PathAttr { D = "M 0 0 L 9 9", FillRule = "evenodd" });
        Assert.Equal("evenodd", path.Attrs["fill-rule"]);
    }

    [Fact]
    public void Canvas_text_takes_attributes_first_and_content_second()
    {
        var node = L.TextAt(
            new CanvasTextAttr { X = "10pt", Y = "20pt", FontSize = "12pt" },
            ["base ", L.Span(new SpanAttr { Font = "Helvetica-Bold" }, ["bold"])]);

        Assert.Equal("text", node.Type);
        Assert.Equal("12pt", node.Attrs["font-size"]);
        Assert.Equal(2, node.Children.Count);
        Assert.IsType<SpanNode>(node.Children[1]);
    }

    [Fact]
    public void Layer_attributes_are_written_as_given()
    {
        var node = L.Layer(new LayerAttr { Page = PageScope.First, Opacity = "0.5", Transform = "rotate(45 100 100)" });

        Assert.Equal("layer", node.Type);
        Assert.Equal("first", node.Attrs["page"]);
        Assert.Equal("0.5", node.Attrs["opacity"]);
        Assert.Equal("rotate(45 100 100)", node.Attrs["transform"]);
    }

    [Fact]
    public void A_transform_is_a_string_a_layer_accepts()
    {
        Assert.Equal("matrix(1,0,0,1,10,20)", new Transform(1, 0, 0, 1, 10, 20).ToString());
    }

    [Fact]
    public async Task A_canvas_shape_is_drawn()
    {
        var empty = L.Document(new DocumentAttr(Size: "a4"), [L.Section(null, [L.Canvas(null, [L.Layer(null, [])])])]);
        var drawn = L.Document(new DocumentAttr(Size: "a4"), [
            L.Section(null, [
                L.Canvas(null, [L.Layer(null, [L.Rect(new RectAttr { X = "50pt", Y = "50pt", W = "200pt", H = "100pt", Fill = "#ff0000" })])]),
            ]),
        ]);

        Assert.NotEqual(await Render(empty), await Render(drawn));
    }

    // -- Rendering ---------------------------------------------------------------------------------

    [Fact]
    public async Task A_built_document_renders_the_same_as_its_xml()
    {
        var built = L.Document(new DocumentAttr(Size: "a4"), [
            L.Section(null, [
                L.Layout(null, [
                    L.Stack(new StackAttr { Gap = "12pt" }, [
                        L.Text(new TextAttr { Align = "right", Bold = "true" }, ["Title"]),
                        L.Text(null, ["Body ", L.Span(new SpanAttr { Bold = "true" }, ["bold"]), " text"]),
                        L.Link(new LinkAttr { Href = "https://lpdf.io" }, [L.Text(null, ["link"])]),
                    ]),
                ]),
                L.Canvas(null, [
                    L.Layer(new LayerAttr { Page = PageScope.Each }, [
                        L.Rect(new RectAttr { X = "40pt", Y = "40pt", W = "100pt", H = "60pt", Fill = "#ff0000", Radius = "6pt" }),
                        L.Circle(new CircleAttr { Cx = "300pt", Cy = "300pt", R = "40pt", Fill = "#00ff00" }),
                        L.TextAt(new CanvasTextAttr { X = "40pt", Y = "120pt", FontSize = "10pt" },
                            ["Canvas ", L.Span(new SpanAttr { Color = "#0000ff" }, ["text"])]),
                    ]),
                ]),
            ]),
        ]);

        Assert.Equal(await Render(built), await Render(await L.ToXml(built)));
    }

    [Fact]
    public async Task Bold_text_is_the_bold_face_of_the_font()
    {
        static string Doc(string attrs) =>
            $"<lpdf version=\"1\"><document><section><layout><text {attrs}>Hello</text></layout></section></document></lpdf>";

        Assert.Equal(await Render(Doc("bold=\"true\"")), await Render(Doc("font=\"Helvetica-Bold\"")));
        Assert.NotEqual(await Render(Doc("bold=\"true\"")), await Render(Doc("")));
    }

    // -- Assets ------------------------------------------------------------------------------------

    private static readonly byte[] Pixel = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private static PdfDocument DocumentWithAssets(DocumentAssets? assets, params Node[] nodes) =>
        L.Document(new DocumentAttr(Assets: assets), [L.Section(L.NoAttr, [L.Layout(L.NoAttr, nodes)])]);

    [Fact]
    public void Assets_declare_fonts_and_images_with_the_schema_names()
    {
        var assets = new DocumentAssets(
            Fonts: [new FontAttr { Name = "heading", Core = BuiltinFont.TimesBold }],
            Images: [new ImageAttr { Name = "logo", Ref = "company-logo", Src = "logo.png" }]);
        var document = L.Document(new DocumentAttr(Assets: assets));

        var declared = Assert.IsType<Dictionary<string, object?>>(document.Attrs["assets"]);
        var fonts = Assert.IsType<List<Dictionary<string, string>>>(declared["fonts"]);
        var images = Assert.IsType<List<Dictionary<string, string>>>(declared["images"]);
        Assert.Equal(new Dictionary<string, string> { ["name"] = "heading", ["core"] = "Times-Bold" }, fonts.Single());
        Assert.Equal(
            new Dictionary<string, string> { ["name"] = "logo", ["ref"] = "company-logo", ["src"] = "logo.png" },
            images.Single());
        Assert.Same(assets, L.Assets(assets));
    }

    [Theory]
    [InlineData(typeof(FontAttr))]
    [InlineData(typeof(ImageAttr))]
    public void An_asset_needs_a_name(Type attributes)
    {
        Assert.NotNull(attributes.GetProperty("Name")!.GetCustomAttribute<RequiredMemberAttribute>());
    }

    [Fact]
    public async Task A_font_declared_in_the_assets_is_the_font_the_text_is_set_in()
    {
        var built = DocumentWithAssets(
            new DocumentAssets(Fonts: [new FontAttr { Name = "heading", Core = BuiltinFont.TimesBold }]),
            L.Text(new TextAttr { Font = "heading" }, ["Hello"]));
        using var engine = L.Engine().SetLicenseKey("test-key");

        Assert.Contains("/BaseFont /Times-Bold", Encoding.Latin1.GetString(await engine.Render(built)));
        Assert.Equal(await Render(built), await Render(await L.ToXml(built)));
    }

    [Fact]
    public async Task An_image_declared_in_the_assets_can_be_used_and_renders_the_same_as_its_xml()
    {
        var built = DocumentWithAssets(
            new DocumentAssets(Images: [new ImageAttr { Name = "logo" }]),
            L.Img(new ImgAttr { Name = "logo", Width = "40pt" }));
        using var engine = L.Engine().SetLicenseKey("test-key").LoadImage("logo", Pixel);

        Assert.Equal(
            Normalised(await engine.Render(built)),
            Normalised(await engine.Render(await L.ToXml(built))));
    }

    [Fact]
    public async Task An_image_used_but_not_declared_in_the_assets_is_an_error_that_names_it()
    {
        var built = DocumentWithAssets(null, L.Img(new ImgAttr { Name = "ghost" }));
        using var engine = L.Engine().SetLicenseKey("test-key").LoadImage("ghost", Pixel);

        var error = await Assert.ThrowsAnyAsync<Exception>(() => engine.Render(built));
        Assert.Contains("ghost", error.Message);
    }

    [Fact]
    public async Task An_image_declared_with_a_src_is_read_through_the_src_fallback()
    {
        var built = DocumentWithAssets(
            new DocumentAssets(Images: [new ImageAttr { Name = "logo", Src = "logo.png" }]),
            L.Img(new ImgAttr { Name = "logo", Width = "40pt" }));
        using var engine = L.Engine(new EngineOptions { SrcFallback = _ => Pixel }).SetLicenseKey("test-key");

        var pdf = await engine.Render(built);

        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdf, 0, 5));
    }

    [Fact]
    public void The_constants_are_the_schema_values()
    {
        Assert.Equal("text", FieldType.Text);
        Assert.Equal("top", Pin.Top);
        Assert.Equal("landscape", Orientation.Landscape);
        Assert.Equal("Times-Bold", BuiltinFont.TimesBold);
    }

    // -- Helpers -----------------------------------------------------------------------------------

    private static async Task<string> Render(PdfDocument document)
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        return Normalised(await engine.Render(document));
    }

    private static async Task<string> Render(string xml)
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        return Normalised(await engine.Render(xml));
    }

    /// <summary>A PDF as text, with the parts that vary left out.</summary>
    private static string Normalised(byte[] pdf)
    {
        var text = Encoding.Latin1.GetString(pdf);
        text = Regex.Replace(text, @"/CreationDate[^\n]*", "");
        return Regex.Replace(text, @"/ID *\[[^\]]*\]", "");
    }
}
