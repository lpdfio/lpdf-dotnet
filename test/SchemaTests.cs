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
        Assert.NotNull(typeof(RectAttr).GetProperty("Width")!.GetCustomAttribute<RequiredMemberAttribute>());
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

    // -- Layers ------------------------------------------------------------------------------------

    [Fact]
    public void Rect_writes_its_attributes_as_given()
    {
        var node = L.Rect(new RectAttr { X = "10pt", Y = "20pt", Width = "100pt", Height = "50pt", Fill = "#ff0000", Radius = "5pt" });

        Assert.Equal("rect", node.Type);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["width"] = "100pt", ["height"] = "50pt", ["x"] = "10pt", ["y"] = "20pt", ["fill"] = "#ff0000", ["radius"] = "5pt",
            },
            node.Attrs);
    }

    [Fact]
    public void Canvas_primitives_use_their_schema_names()
    {
        Assert.Equal("line", L.Line(new LineAttr { X1 = "0pt", Y1 = "0pt", X2 = "9pt", Y2 = "9pt", LineCap = "round" }).Type);
        Assert.Equal("circle", L.Circle(new CircleAttr { R = "5pt", X = "1pt", Y = "1pt" }).Type);
        Assert.Equal("ellipse", L.Ellipse(new EllipseAttr { Rx = "5pt", Ry = "3pt" }).Type);
        Assert.Equal("img-at", L.ImgAt(new ImgAtAttr { Name = "logo", Width = "10pt", Height = "10pt" }).Type);

        var path = L.Path(new PathAttr { D = "M 0 0 L 9 9", FillRule = "evenodd" });
        Assert.Equal("evenodd", path.Attrs["fill-rule"]);
    }

    [Fact]
    public void Canvas_text_takes_attributes_first_and_content_second()
    {
        var node = L.TextAt(
            new TextAtAttr { X = "10pt", Y = "20pt", FontSize = "12pt" },
            ["base ", L.Span(new SpanAttr { Font = "Helvetica-Bold" }, ["bold"])]);

        Assert.Equal("text-at", node.Type);
        Assert.Equal("12pt", node.Attrs["font-size"]);
        Assert.Equal(2, node.Children.Count);
        Assert.IsType<SpanNode>(node.Children[1]);
    }

    [Fact]
    public void Layer_attributes_are_written_as_given()
    {
        var node = L.Layer(new LayerAttr { Page = PageScope.First, Opacity = "0.5", Transform = "rotate(45 100 100)", Clip = "0pt 0pt 200pt 100pt" });

        Assert.Equal("layer", node.Type);
        Assert.Equal("first", node.Attrs["page"]);
        Assert.Equal("0.5", node.Attrs["opacity"]);
        Assert.Equal("rotate(45 100 100)", node.Attrs["transform"]);
        Assert.Equal("0pt 0pt 200pt 100pt", node.Attrs["clip"]);
    }

    [Fact]
    public void A_layer_gives_its_text_a_font_a_size_and_a_color_under_their_schema_names()
    {
        var node = L.Layer(new LayerAttr { Font = "Courier", FontSize = "s", Color = "danger" });

        Assert.Equal("Courier", node.Attrs["font"]);
        Assert.Equal("s", node.Attrs["font-size"]);
        Assert.Equal("danger", node.Attrs["color"]);
    }

    [Fact]
    public void A_box_has_a_color_and_an_opacity()
    {
        var node = L.Column(new ColumnAttr { Color = "text-muted", Opacity = "0.5" });

        Assert.Equal("text-muted", node.Attrs["color"]);
        Assert.Equal("0.5", node.Attrs["opacity"]);
    }

    [Fact]
    public void A_box_and_a_layer_shape_have_a_shadow()
    {
        Assert.Equal("2pt 3pt #000000 0.2", L.Column(new ColumnAttr { Shadow = "2pt 3pt #000000 0.2" }).Attrs["shadow"]);
        Assert.Equal("2pt 3pt #000000", L.Rect(new RectAttr { Width = "50pt", Height = "30pt", Shadow = "2pt 3pt #000000" }).Attrs["shadow"]);
        Assert.Equal("-2pt -3pt text", L.Circle(new CircleAttr { R = "5pt", Shadow = "-2pt -3pt text" }).Attrs["shadow"]);
        Assert.Equal("2pt 3pt #000000 0.5", L.Ellipse(new EllipseAttr { Rx = "5pt", Ry = "3pt", Shadow = "2pt 3pt #000000 0.5" }).Attrs["shadow"]);
    }

    [Fact]
    public void A_transform_is_a_string_a_layer_accepts()
    {
        Assert.Equal("matrix(1,0,0,1,10,20)", new Transform(1, 0, 0, 1, 10, 20).ToString());
    }

    [Fact]
    public async Task A_layer_shape_is_drawn()
    {
        var empty = L.Document(new DocumentAttr(Size: "a4"), [L.Section(null, [L.Layer(null, [])])]);
        var drawn = L.Document(new DocumentAttr(Size: "a4"), [
            L.Section(null, [
                L.Layer(null, [L.Rect(new RectAttr { X = "50pt", Y = "50pt", Width = "200pt", Height = "100pt", Fill = "#ff0000" })]),
            ]),
        ]);

        Assert.NotEqual(await Render(empty), await Render(drawn));
    }

    [Fact]
    public async Task Layers_go_directly_in_a_section_before_and_after_its_layout()
    {
        var section = L.Section(null, [
            L.Layer(new LayerAttr { Page = PageScope.First }, [L.Rect(new RectAttr { Width = "10pt", Height = "10pt" })]),
            L.Layout(null, [L.Text(null, ["Body"])]),
            L.Layer(new LayerAttr { Page = PageScope.Last }, [L.Rect(new RectAttr { Width = "20pt", Height = "20pt" })]),
        ]);
        Assert.Equal(["layer", "layout", "layer"], section.Nodes.Select(n => n.Type));

        var xml = await L.ToXml(L.Document(null, [section]));
        var under = xml.IndexOf("<layer page=\"first\">", StringComparison.Ordinal);
        var layout = xml.IndexOf("<layout>", StringComparison.Ordinal);
        var over = xml.IndexOf("<layer page=\"last\">", StringComparison.Ordinal);
        Assert.True(under >= 0 && under < layout && layout < over, xml);
        Assert.DoesNotContain("<canvas", xml);
    }

    [Fact]
    public async Task A_section_with_two_layouts_is_rejected()
    {
        var doc = L.Document(null, [
            L.Section(null, [L.Layout(null, [L.Text(null, ["a"])]), L.Layout(null, [L.Text(null, ["b"])])]),
        ]);

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(doc));
        Assert.Contains("at most one <layout>", error.Message);
    }

    [Fact]
    public void A_layer_shape_is_sized_by_width_and_height_and_placed_by_x_and_y()
    {
        foreach (var attr in new[] { typeof(RectAttr), typeof(ImgAtAttr), typeof(TextAtAttr) })
        {
            Assert.Null(attr.GetProperty("W"));
            Assert.NotNull(attr.GetProperty("Width"));
        }
        Assert.Null(typeof(RectAttr).GetProperty("H"));
        Assert.NotNull(typeof(RectAttr).GetProperty("Height"));

        foreach (var attr in new[] { typeof(CircleAttr), typeof(EllipseAttr) })
        {
            Assert.Null(attr.GetProperty("Cx"));
            Assert.Null(attr.GetProperty("Cy"));
            Assert.NotNull(attr.GetProperty("X"));
            Assert.NotNull(attr.GetProperty("Y"));
        }
    }

    [Fact]
    public void A_barcode_at_is_written_under_its_element_name()
    {
        var node = L.BarcodeAt(new BarcodeAtAttr { Type = "qr", Data = "x", X = "10pt", Y = "20pt", Width = "60pt" });

        Assert.Equal("barcode-at", node.Type);
        Assert.Equal("60pt", node.Attrs["width"]);
    }

    [Fact]
    public async Task A_text_at_is_written_under_its_element_name_with_its_content()
    {
        var doc = L.Document(null, [L.Section(null, [L.Layer(null, [
            L.TextAt(new TextAtAttr { X = "10pt", Y = "20pt", MaxLines = "2", Width = "80pt" }, ["Page {page} of {pages}"]),
        ])])]);

        var xml = await L.ToXml(doc);
        Assert.Contains("<text-at ", xml);
        Assert.Contains("max-lines=\"2\"", xml);
        Assert.Contains(">Page {page} of {pages}</text-at>", xml);
    }

    [Fact]
    public void A_text_that_captures_and_one_that_shows_a_capture_are_written_under_the_schema_names()
    {
        var source = L.Text(new TextAttr { Capture = "chapter" }, ["x"]);
        Assert.Equal("chapter", source.Attrs["capture"]);

        var sink = L.Text(new TextAttr { CaptureRef = "chapter", CaptureMode = "first", CaptureFallback = "None" }, []);
        Assert.Equal("chapter", sink.Attrs["capture-ref"]);
        Assert.Equal("first", sink.Attrs["capture-mode"]);
        Assert.Equal("None", sink.Attrs["capture-fallback"]);
    }

    [Fact]
    public void A_barcode_is_sized_by_width_and_height_not_by_size()
    {
        Assert.Null(typeof(BarcodeAttr).GetProperty("Size"));
        Assert.NotNull(typeof(BarcodeAttr).GetProperty("Width"));
        Assert.NotNull(typeof(BarcodeAttr).GetProperty("Height"));
    }

    [Fact]
    public async Task A_circle_is_written_with_x_and_y_and_drawn_where_its_box_is()
    {
        var circleAt = (string x) => L.Document(new DocumentAttr(Size: "a4"), [
            L.Section(null, [L.Layer(null, [L.Circle(new CircleAttr { X = x, Y = "20pt", R = "10pt", Fill = "#ff0000" })])]),
        ]);

        var xml = await L.ToXml(circleAt("20pt"));
        Assert.Contains("<circle ", xml);
        Assert.Contains("x=\"20pt\"", xml);
        Assert.DoesNotContain("cx=", xml);
        Assert.NotEqual(await Render(circleAt("20pt")), await Render(circleAt("120pt")));
    }

    [Fact]
    public async Task The_margin_is_written_on_the_layout()
    {
        var doc = L.Document(null, [L.Section(null, [L.Layout(new() { Margin = "48pt" }, [L.Text(null, ["Body"])])])]);

        Assert.Contains("<layout margin=\"48pt\">", await L.ToXml(doc));
    }

    // -- Rendering ---------------------------------------------------------------------------------

    [Fact]
    public async Task A_built_document_renders_the_same_as_its_xml()
    {
        var built = L.Document(new DocumentAttr(Size: "a4"), [
            L.Section(null, [
                L.Layout(null, [
                    L.Column(new ColumnAttr { Gap = "12pt" }, [
                        L.Text(new TextAttr { Align = "right", Bold = "true" }, ["Title"]),
                        L.Text(null, ["Body ", L.Span(new SpanAttr { Bold = "true" }, ["bold"]), " text"]),
                        L.Link(new LinkAttr { Href = "https://lpdf.io" }, [L.Text(null, ["link"])]),
                    ]),
                ]),
                L.Layer(new LayerAttr { Page = PageScope.Each }, [
                    L.Rect(new RectAttr { X = "40pt", Y = "40pt", Width = "100pt", Height = "60pt", Fill = "#ff0000", Radius = "6pt" }),
                    L.Circle(new CircleAttr { X = "260pt", Y = "260pt", R = "40pt", Fill = "#00ff00" }),
                    L.TextAt(new TextAtAttr { X = "40pt", Y = "120pt", FontSize = "10pt" },
                        ["Layer ", L.Span(new SpanAttr { Color = "#0000ff" }, ["text"])]),
                ]),
            ]),
        ]);

        Assert.Equal(await Render(built), await Render(await L.ToXml(built)));
    }

    [Fact]
    public async Task An_alias_is_the_slot_it_names_and_renders_the_same_from_the_builder_and_its_XML()
    {
        PdfDocument Build(string size) => L.Document(
            new DocumentAttr(Tokens: new DocumentTokens(Aliases: new() { ["caption"] = "xs", ["heading"] = "xl" })),
            [L.Section(nodes: [L.Layout(null, [L.Text(new TextAttr { FontSize = size }, ["Sized"])])])]);

        var caption = Build("caption");
        var pdf = await Render(caption);

        Assert.Equal(pdf, await Render(await L.ToXml(caption)));
        Assert.Equal(pdf, await Render(Build("xs")));
        Assert.NotEqual(pdf, await Render(Build("heading")));
    }

    [Fact]
    public async Task A_name_that_is_neither_a_slot_nor_an_alias_is_rejected()
    {
        var doc = L.Document(
            new DocumentAttr(Tokens: new DocumentTokens(Aliases: new() { ["caption"] = "xs" })),
            [L.Section(nodes: [L.Layout(null, [L.Text(new TextAttr { FontSize = "medum" }, ["Sized"])])])]);

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(doc));
        Assert.Contains("invalid text size: 'medum': expected a slot from xs to xxl, an alias (caption)", error.Message);
    }

    [Fact]
    public async Task A_height_is_the_slot_it_names_and_renders_the_same_from_the_builder_and_its_XML()
    {
        PdfDocument Build(string height, Dictionary<string, string>? row = null) => L.Document(
            new DocumentAttr(Tokens: row is null ? null : new DocumentTokens(Height: row)),
            [L.Section(nodes: [L.Layout(null, [L.Column(new ColumnAttr { Background = "#cccccc", Height = height })])])]);

        var slot = Build("m");
        var pdf = await Render(slot);

        Assert.Equal(pdf, await Render(await L.ToXml(slot)));
        Assert.Equal(pdf, await Render(Build("32pt")));
        Assert.NotEqual(pdf, await Render(Build("xl")));

        var row = new Dictionary<string, string>
        {
            ["xs"] = "10pt", ["s"] = "20pt", ["m"] = "50pt", ["l"] = "60pt", ["xl"] = "70pt", ["xxl"] = "80pt",
        };
        Assert.Equal(await Render(Build("50pt")), await Render(Build("m", row)));
    }

    [Fact]
    public async Task A_height_that_is_not_a_slot_a_length_fill_or_full_is_rejected()
    {
        var doc = L.Document(
            nodes: [L.Section(nodes: [L.Layout(null, [L.Column(new ColumnAttr { Height = "medum" })])])]);

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(doc));
        Assert.Contains("invalid height value: 'medum' — expected full, fill, a slot from xs to xxl or a pt value", error.Message);
    }

    [Fact]
    public async Task A_gradient_of_the_tokens_fills_a_box_and_renders_the_same_from_the_builder_and_its_XML()
    {
        PdfDocument Build(string background) => L.Document(
            new DocumentAttr(Tokens: new DocumentTokens(Gradients: new()
            {
                ["fade"] = new() { ["type"] = "linear", ["from"] = "#ff0000", ["to"] = "#0000ff", ["angle"] = 90 },
            })),
            [L.Section(nodes: [L.Layout(null, [L.Column(new ColumnAttr { Background = background, Height = "60pt" })])])]);

        var gradient = Build("gradient:fade");
        var pdf = await Render(gradient);

        Assert.Equal(pdf, await Render(await L.ToXml(gradient)));
        Assert.NotEqual(pdf, await Render(Build("#336699")));
        Assert.Contains("/ShadingType 2", pdf);
    }

    [Fact]
    public async Task Bold_text_is_the_bold_face_of_the_font()
    {
        static string Doc(string attrs) =>
            $"<lpdf version=\"1\"><document><section><layout><text {attrs}>Hello</text></layout></section></document></lpdf>";

        Assert.Equal(await Render(Doc("bold=\"true\"")), await Render(Doc("font=\"Helvetica-Bold\"")));
        Assert.NotEqual(await Render(Doc("bold=\"true\"")), await Render(Doc("")));
    }

    // -- Bookmarks and the table of contents -------------------------------------------------------

    [Fact]
    public void Bookmark_goes_in_a_layout_and_directly_in_a_section()
    {
        var bookmark = L.Bookmark(new BookmarkAttr { Label = "Methods", Level = "2", Id = "methods" });
        Assert.Equal(new Dictionary<string, string> { ["label"] = "Methods", ["level"] = "2", ["id"] = "methods" }, bookmark.Attrs);

        var section = L.Section(new SectionAttr(Type: SectionType.Toc, TocIndentStep: "14pt"), [
            L.Bookmark(new BookmarkAttr { Label = "Contents" }),
            L.Layout(null, [bookmark, L.Text(null, ["Methods"])]),
        ]);
        Assert.Equal(new Dictionary<string, string> { ["type"] = "toc", ["toc-indent-step"] = "14pt" }, section.Attrs);
        Assert.Equal("bookmark", Assert.IsType<SectionBookmark>(section.Nodes[0]).Type);
    }

    [Fact]
    public async Task Bookmarks_fill_the_outline_and_render_like_their_xml()
    {
        var built = L.Document(null, [
            L.Section(null, [L.Bookmark(new BookmarkAttr { Label = "Cover" }), L.Layout(null, [L.Text(null, ["Cover"])])]),
            L.Section(null, [
                L.Layout(null, [
                    L.Link(new LinkAttr { Href = "#methods" }, [L.Text(null, ["Go to Methods"])]),
                    L.Bookmark(new BookmarkAttr { Label = "Methods" }),
                    L.Text(null, ["Methods"]),
                ]),
            ]),
        ]);
        var pdf = await Render(built);

        Assert.Contains("/Title (Cover)", pdf);
        Assert.Contains("/Title (Methods)", pdf);
        Assert.Contains("/D /methods", pdf);
        Assert.Equal(pdf, await Render(await L.ToXml(built)));
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
