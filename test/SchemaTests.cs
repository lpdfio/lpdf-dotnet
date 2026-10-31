using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Lpdf;
using Lpdf.Canvas;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Layout;
using Lpdf.Markdown;
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
        Assert.Equal(new[] { "source", "data", "alternative", "supplement", "unspecified" },
            new[] { FileRelationship.Source, FileRelationship.Data, FileRelationship.Alternative, FileRelationship.Supplement, FileRelationship.Unspecified });
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
    public void A_shape_has_the_attributes_of_its_type_under_the_schema_names()
    {
        var node = L.Shape(new ShapeAttr
        {
            Type = "rect", Width = "14pt", Height = "m", Radius = "2pt", Fill = "primary",
            Stroke = "#000000", StrokeWidth = "1pt", StrokeDash = "3 2", Opacity = "0.5",
        });

        Assert.Equal("shape", node.Type);
        Assert.Equal(
            new Dictionary<string, string>
            {
                ["type"] = "rect", ["width"] = "14pt", ["height"] = "m", ["radius"] = "2pt", ["fill"] = "primary",
                ["stroke"] = "#000000", ["stroke-width"] = "1pt", ["stroke-dash"] = "3 2", ["opacity"] = "0.5",
            },
            node.Attrs);
        Assert.Equal("5pt", L.Shape(new ShapeAttr { Type = "circle", R = "5pt" }).Attrs["r"]);
    }

    [Fact]
    public async Task A_shape_of_each_type_is_drawn_and_renders_the_same_from_the_builder_and_its_XML()
    {
        PdfDocument Build(params Node[] shapes) => L.Document(
            nodes: [L.Section(nodes: [L.Layout(null, [L.Row(new RowAttr { Align = "center" }, [.. shapes, L.Text(null, ["After"])])])])]);

        var without = await Render(Build());
        foreach (var shape in new Node[]
        {
            L.Shape(new ShapeAttr { Type = "rect", Width = "60pt", Height = "12pt", Fill = "#00aa00" }),
            L.Shape(new ShapeAttr { Type = "circle", R = "8pt", Fill = "#00aa00" }),
            L.Shape(new ShapeAttr { Type = "ellipse", Width = "40pt", Height = "20pt", Fill = "#00aa00" }),
        })
        {
            var document = Build(shape);
            Assert.NotEqual(without, await Render(document));
            Assert.Equal(await Render(document), await Render(await L.ToXml(document)));
        }
    }

    [Fact]
    public async Task A_kept_table_row_moves_on_with_the_row_after_it_and_renders_the_same_from_the_builder_and_its_XML()
    {
        // The table starts 30pt above the foot of the page, so rows A and B fit and C does not.
        PdfDocument Build(TrAttr? keep) => L.Document(nodes: [L.Section(nodes: [L.Layout(null, [
            L.Frame(new FrameAttr { Height = "739pt" }),
            L.Table(new TableAttr { Cols = "1fr" }, [
                L.Tr(null, [L.Td(null, [L.Text(null, ["A"])])]),
                L.Tr(keep, [L.Td(null, [L.Text(null, ["B"])])]),
                L.Tr(null, [L.Td(null, [L.Text(null, ["C"])])]),
            ]),
        ])])]);

        var kept = Build(new TrAttr { Keep = "with-next" });

        Assert.Equal("with-next", L.Tr(new TrAttr { Keep = "with-next" }).Attrs["keep"]);
        Assert.NotEqual(await Render(Build(null)), await Render(kept));
        Assert.Equal(await Render(kept), await Render(await L.ToXml(kept)));
    }

    [Fact]
    public async Task A_table_cell_that_spans_columns_moves_the_cells_after_it_and_renders_the_same_from_the_builder_and_its_XML()
    {
        PdfDocument Build(TdAttr? first) => L.Document(nodes: [L.Section(nodes: [L.Layout(null, [
            L.Table(new TableAttr { Cols = "1fr 1fr 1fr" }, [
                L.Tr(null, [L.Td(first, [L.Text(null, ["A"])]), L.Td(null, [L.Text(null, ["B"])])]),
            ]),
        ])])]);

        var spanning = Build(new TdAttr { Colspan = "2" });

        Assert.Equal("2", L.Td(new TdAttr { Colspan = "2" }).Attrs["colspan"]);
        Assert.NotEqual(await Render(Build(null)), await Render(spanning));
        Assert.Equal(await Render(spanning), await Render(await L.ToXml(spanning)));

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(Build(new TdAttr { Colspan = "3" })));
        Assert.Contains("span 4 columns, but the table has 3", error.Message);
    }

    [Fact]
    public async Task A_table_cell_that_spans_rows_moves_the_cells_under_it_and_renders_the_same_from_the_builder_and_its_XML()
    {
        PdfDocument Build(TdAttr? first) => L.Document(nodes: [L.Section(nodes: [L.Layout(null, [
            L.Table(new TableAttr { Cols = "1fr 1fr" }, [
                L.Tr(null, [L.Td(first, [L.Text(null, ["A"])]), L.Td(null, [L.Text(null, ["B"])])]),
                L.Tr(null, [L.Td(null, [L.Text(null, ["C"])])]),
            ]),
        ])])]);

        var spanning = Build(new TdAttr { Rowspan = "2" });

        Assert.Equal("2", L.Td(new TdAttr { Rowspan = "2" }).Attrs["rowspan"]);
        Assert.NotEqual(await Render(Build(null)), await Render(spanning));
        Assert.Equal(await Render(spanning), await Render(await L.ToXml(spanning)));

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(Build(new TdAttr { Rowspan = "3" })));
        Assert.Contains("a rowspan that runs 1 row past the last row of the table", error.Message);
    }

    [Fact]
    public async Task A_footnote_is_an_fn_child_of_a_text_and_the_counter_is_an_attribute_of_the_document_and_a_section()
    {
        var text = L.Text(L.NoAttr, ["Total", L.Fn(L.NoAttr, ["See ", L.Span(new SpanAttr { Bold = "true" }, ["this"]), "."]), " here"]);
        var document = L.Document(
            new DocumentAttr(FootnoteCounter: FootnoteCounter.Document),
            [L.Section(new SectionAttr(FootnoteCounter: FootnoteCounter.Page), [L.Layout(null, [text])])]);

        var xml = await L.ToXml(document);
        Assert.Matches("<document [^>]*footnote-counter=\"document\"", xml);
        Assert.Matches("<section [^>]*footnote-counter=\"page\"", xml);
        Assert.Contains("<text>Total<fn>See <span bold=\"true\">this</span>.</fn> here</text>", xml);
        Assert.Equal("fn", L.Fn().Type);
        Assert.Empty(L.Fn().Attrs);
    }

    [Fact]
    public async Task A_footnote_is_drawn_and_renders_the_same_from_the_builder_and_its_XML_and_one_in_a_table_is_rejected()
    {
        PdfDocument Build(params TextContent[] nodes) => L.Document(
            nodes: [L.Section(nodes: [L.Layout(null, [L.Text(null, ["Total", .. nodes, " here"])])])]);

        var without = await Render(Build());
        var noted = Build(L.Fn(L.NoAttr, ["The note."]));
        Assert.NotEqual(without, await Render(noted));
        Assert.Equal(await Render(noted), await Render(await L.ToXml(noted)));

        PdfDocument Counted(string counter) => L.Document(
            new DocumentAttr(FootnoteCounter: counter),
            [
                L.Section(nodes: [L.Layout(null, [L.Text(null, ["One", L.Fn(L.NoAttr, ["A."])])])]),
                L.Section(nodes: [L.Layout(null, [L.Text(null, ["Two", L.Fn(L.NoAttr, ["B."])])])]),
            ]);
        Assert.NotEqual(await Render(Counted(FootnoteCounter.Document)), await Render(Counted(FootnoteCounter.Section)));

        var table = L.Document(nodes: [L.Section(nodes: [L.Layout(null, [
            L.Table(new TableAttr { Cols = "1fr" }, [L.Tr(null, [L.Td(null, [L.Text(null, ["x", L.Fn(L.NoAttr, ["y"])])])])])])])]);
        var error = await Assert.ThrowsAsync<EngineException>(() => Render(table));
        Assert.Contains("<fn> is not supported in a table", error.Message);
        var bad = await Assert.ThrowsAsync<EngineException>(() => Render(Counted("book")));
        Assert.Contains("invalid footnote-counter 'book'", bad.Message);
    }

    [Fact]
    public async Task A_code_and_a_code_span_are_written_as_their_elements_and_render_the_same_from_the_builder_and_its_XML()
    {
        var document = L.Document(nodes: [L.Section(nodes: [L.Layout(null, [
            L.Code(new CodeAttr { TabSize = "2" }, ["\nfn a() {\n\tb(", L.Span(new SpanAttr { Color = "#c0392b" }, ["x"]), ");\n}\n"]),
            L.Text(null, ["Run ", L.CodeSpan(new CodeSpanAttr { Background = "#ffff00" }, ["cargo test"]), " now"]),
        ])])]);

        var xml = await L.ToXml(document);
        Assert.Contains("<code tab-size=\"2\">\nfn a() {\n\tb(<span color=\"#c0392b\">x</span>);\n}\n</code>", xml);
        Assert.Contains("Run <code-span background=\"#ffff00\">cargo test</code-span> now", xml);
        Assert.Equal("code", L.Code().Type);
        Assert.Equal("code-span", L.CodeSpan().Type);
        Assert.Equal(await Render(document), await Render(xml));
    }

    [Fact]
    public async Task A_code_keeps_its_spaces_and_one_longer_than_a_page_goes_on_in_the_next_and_a_bad_tab_size_is_rejected()
    {
        PdfDocument Build(params TextContent[] nodes) => L.Document(
            nodes: [L.Section(nodes: [L.Layout(null, [L.Code(null, nodes)])])]);

        Assert.NotEqual(await Render(Build("a    b")), await Render(Build("a b")));

        var lines = string.Join("\n", Enumerable.Range(1, 150).Select(i => $"line {i}"));
        var pdf = await Render(Build(lines));
        Assert.True(System.Text.RegularExpressions.Regex.Matches(pdf, @"/Type\s*/Page[^s]").Count >= 2, "the code splits across pages");

        var document = L.Document(nodes: [L.Section(nodes: [L.Layout(null, [L.Code(new CodeAttr { TabSize = "0" }, ["x"])])])]);
        var error = await Assert.ThrowsAsync<EngineException>(() => Render(document));
        Assert.Contains("invalid tab-size", error.Message);
    }

    [Fact]
    public async Task A_list_is_written_with_its_items_and_renders_the_same_from_the_builder_and_its_XML()
    {
        var document = L.Document(nodes: [L.Section(nodes: [L.Layout(null, [
            L.List(new ListAttr { Type = "number", Format = "lower-alpha", Suffix = ")" }, [
                L.Li(null, [L.Text(null, ["One"])]),
                L.Li(null, [L.Text(null, ["Two"]), L.List(new ListAttr { Bullet = "dash" }, [L.Li(null, [L.Text(null, ["Nested"])])])]),
            ]),
        ])])]);

        var xml = await L.ToXml(document);
        Assert.Matches("<list format=\"lower-alpha\" suffix=\"\\)\" type=\"number\">\\s*<li>\\s*<text>One</text>\\s*</li>", xml);
        Assert.Matches("<list bullet=\"dash\">\\s*<li>\\s*<text>Nested</text>", xml);
        Assert.Equal("list", L.List().Type);
        Assert.Equal("li", L.Li().Type);
        Assert.Equal(await Render(document), await Render(xml));
    }

    [Fact]
    public async Task A_number_list_writes_its_numbers_and_a_bullet_list_refuses_what_numbers_them()
    {
        Node[] Items() => [L.Li(null, [L.Text(null, ["A"])]), L.Li(null, [L.Text(null, ["B"])])];
        PdfDocument Build(ListAttr? attrs) => L.Document(nodes: [L.Section(nodes: [L.Layout(null, [L.List(attrs, Items())])])]);

        var bullets = await Render(Build(null));
        var numbers = await Render(Build(new ListAttr { Type = "number" }));
        var roman   = await Render(Build(new ListAttr { Type = "number", Format = "lower-roman", Start = "3" }));
        Assert.NotEqual(bullets, numbers);
        Assert.NotEqual(numbers, roman);

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(Build(new ListAttr { Format = "lower-alpha" })));
        Assert.Contains("numbers the items of a list", error.Message);
    }

    [Fact]
    public async Task A_circle_without_r_is_rejected_and_says_so()
    {
        var doc = L.Document(
            nodes: [L.Section(nodes: [L.Layout(null, [L.Shape(new ShapeAttr { Type = "circle" })])])]);

        var error = await Assert.ThrowsAsync<EngineException>(() => Render(doc));
        Assert.Contains("a circle needs 'r'", error.Message);
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

    // -- Markdown ----------------------------------------------------------------------------------

    private const string MarkdownBody = "# Release notes\n\nSome *emphasis*, **strong** text and `code`.\n\n- one\n- two\n\n> a quote\n";

    private static PdfDocument MarkdownDocument(MarkdownAttr? attrs = null, MarkdownStylesNode? styles = null, string text = MarkdownBody)
    {
        var parts = new List<MarkdownPart>();
        if (styles is not null) parts.Add(styles);
        parts.Add(LMarkdown.Content(L.NoAttr, [text]));
        return L.Document(nodes: [L.Section(nodes: [L.Markdown(attrs, [.. parts])])]);
    }

    [Fact]
    public void A_markdown_its_styles_its_content_and_their_rules_are_built_with_the_schema_names()
    {
        var markdown = L.Markdown(new MarkdownAttr { Margin = "54pt", Gap = "m", Bookmarks = "2" });
        Assert.Equal("markdown", markdown.Type);
        Assert.Equal(new Dictionary<string, string> { ["margin"] = "54pt", ["gap"] = "m", ["bookmarks"] = "2" }, markdown.Attrs);

        var styles = LMarkdown.Styles(new MarkdownStylesAttr { Src = "styles.xml" }, [LMarkdown.H1(new MarkdownH1Attr { FontSize = "xl", Color = "muted" })]);
        Assert.Equal("markdown-styles", styles.Type);
        Assert.Equal("h1", styles.Rules[0].RuleType);
        Assert.Equal(new Dictionary<string, string> { ["font-size"] = "xl", ["color"] = "muted" }, styles.Rules[0].Attrs);
        Assert.Equal("quote", LMarkdown.Quote(new MarkdownQuoteAttr { BarColor = "primary" }).RuleType);
        Assert.Empty(LMarkdown.Em(L.NoAttr).Attrs);
        Assert.Equal(["# a"], LMarkdown.Content(L.NoAttr, ["# a"]).Text);
    }

    [Fact]
    public async Task A_markdown_is_written_as_XML_with_its_Markdown_as_it_is_and_renders_the_same_from_the_builder_and_its_XML()
    {
        var document = MarkdownDocument(
            new MarkdownAttr { Gap = "8pt" },
            LMarkdown.Styles(L.NoAttr, [LMarkdown.H1(new MarkdownH1Attr { Color = "#ff0000" })]),
            "a < b & c ]]> d\n\n    code\n");

        var xml = await L.ToXml(document);
        Assert.Contains("<markdown gap=\"8pt\">", xml);
        Assert.Matches("<markdown-styles>\\s*<h1 color=\"#ff0000\"\\s*/>\\s*</markdown-styles>", xml);
        Assert.Contains("<markdown-content><![CDATA[a < b & c ]]]]><![CDATA[> d", xml);
        Assert.Equal(await Render(document), await Render(xml));
    }

    [Fact]
    public async Task A_markdown_renders_the_text_it_is_given_and_a_rule_that_sets_a_color_changes_the_page()
    {
        var plain = await Render(MarkdownDocument());
        Assert.StartsWith("%PDF-", plain);
        Assert.NotEqual(plain, await Render(MarkdownDocument(styles: LMarkdown.Styles(L.NoAttr, [LMarkdown.H1(new MarkdownH1Attr { Color = "#ff0000" })]))));
        Assert.NotEqual(plain, await Render(MarkdownDocument(text: "# Another title")));
    }

    [Fact]
    public async Task A_file_of_rules_is_read_from_the_src_of_its_styles_or_loaded_by_name()
    {
        var inline = await Render(MarkdownDocument(styles: LMarkdown.Styles(L.NoAttr, [LMarkdown.H1(new MarkdownH1Attr { Color = "#ff0000" })])));
        var fromSrc = MarkdownDocument(styles: LMarkdown.Styles(new MarkdownStylesAttr { Src = "styles.xml" }));
        const string File = "<markdown-styles><h1 color=\"#ff0000\"/></markdown-styles>";

        using (var engine = L.Engine(new EngineOptions { SrcFallback = _ => Encoding.UTF8.GetBytes(File) }).SetLicenseKey("test-key"))
        {
            Assert.Equal(inline, Normalised(await engine.Render(fromSrc)));
            Assert.Equal(inline, Normalised(await engine.Render(await L.ToXml(fromSrc))));
        }
        using (var engine = L.Engine().SetLicenseKey("test-key").LoadMarkdownStyles("styles.xml", File))
        {
            Assert.Equal(inline, Normalised(await engine.Render(fromSrc)));
        }
    }

    [Fact]
    public async Task A_file_of_rules_that_is_not_there_is_an_error_that_names_the_src()
    {
        var missing = await Assert.ThrowsAsync<EngineException>(() => Render(MarkdownDocument(styles: LMarkdown.Styles(new MarkdownStylesAttr { Src = "missing-styles.xml" }))));
        Assert.Contains("missing-styles.xml", missing.Message);
    }

    [Fact]
    public async Task Raw_HTML_is_left_out_of_the_Markdown_and_the_words_in_it_are_kept()
    {
        var withHtml = await Render(MarkdownDocument(text: "fine\n\n<div>kept</div>\n"));
        Assert.Equal(await Render(MarkdownDocument(text: "fine\n\nkept\n")), withHtml);
    }

    [Fact]
    public async Task The_Markdown_comes_from_the_data_of_an_XML_document_or_the_text_written_in_it()
    {
        const string xml = "<lpdf version=\"1\"><document><section><markdown><markdown-content data-value=\"body\"><![CDATA[# From the text]]></markdown-content></markdown></section></document></lpdf>";
        using var engine = L.Engine().SetLicenseKey("test-key");
        var fromData = Normalised(await engine.Render(xml, new RenderOptions { Data = new Dictionary<string, object?> { ["body"] = "# From the data" } }));
        var fromText = Normalised(await engine.Render(xml));
        Assert.NotEqual(fromData, fromText);
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
