using Lpdf.Canvas;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Layout;
using Lpdf.Shared;
using System.Text.Json;

namespace Lpdf;

/// <summary>
/// Flat entry point for building and rendering lpdf documents.
///
/// <example>
/// <code>
/// using Lpdf;
/// using Lpdf.Layout;
///
/// var doc = Pdf.Document(new DocumentAttr(Size: "a4"), [
///     Pdf.Section(null, [
///         Pdf.Layout(null, [
///             Pdf.Text([Pdf.Raw("Hello")], new TextAttr(Font: "heading")),
///         ])
///     ])
/// ]);
/// var bytes = await Pdf.Engine().SetLicenseKey("…").Render(doc);
/// </code>
/// </example>
/// </summary>
public static class Pdf
{
    /// <summary>No attributes — pass as the <c>attrs</c> argument for container nodes that need none.</summary>
    public const object? NoAttr = null;

    // ── Engine ────────────────────────────────────────────────────────────────

    /// <summary>Create a new <see cref="PdfEngine"/> instance.</summary>
    public static PdfEngine Engine(EngineOptions? options = null)
        => new(options);

    // ── XML conversion ────────────────────────────────────────────────────────

    /// <summary>Convert a <see cref="PdfDocument"/> tree to an lpdf XML string without rendering it.</summary>
    public static Task<string> ToXml(PdfDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);
        var json = JsonSerializer.Serialize(document, DocumentJson.Options);
        var wasm = new WasmRunner();
        try { return Task.FromResult(wasm.KitToXml(json)); }
        finally { wasm.Dispose(); }
    }

    // ── Document / section ────────────────────────────────────────────────────

    /// <summary>Build the root <c>document</c> node.</summary>
    public static PdfDocument Document(DocumentAttr? attrs = null, SectionNode[]? nodes = null)
    {
        var a = attrs ?? new DocumentAttr();
        var d = new Dictionary<string, object?>(StringComparer.Ordinal);

        if (a.Size        is not null) d["size"]        = a.Size;
        if (a.Orientation is not null) d["orientation"] = a.Orientation;
        if (a.Margin      is not null) d["margin"]      = a.Margin;
        if (a.Background  is not null) d["background"]  = a.Background;

        if (a.Tokens is not null)
        {
            var t = a.Tokens;
            var td = new Dictionary<string, object?>(StringComparer.Ordinal);
            if (t.Colors is not null) td["colors"] = t.Colors;
            if (t.Space  is not null) td["space"]  = t.Space;
            if (t.Grid   is not null) td["grid"]   = t.Grid;
            if (t.Border is not null) td["border"] = t.Border;
            if (t.Radius is not null) td["radius"] = t.Radius;
            if (t.Width  is not null) td["width"]  = t.Width;
            if (t.Text   is not null) td["text"]   = t.Text;
            if (t.Fonts  is not null)
            {
                var fonts = new Dictionary<string, object?>(StringComparer.Ordinal);
                foreach (var (name, def) in t.Fonts)
                    fonts[name] = def switch
                    {
                        FontSrc     src     => (object)new { src     = src.Src },
                        FontBuiltin builtin => (object)new { builtin = builtin.Builtin },
                        _                   => null,
                    };
                td["fonts"] = fonts;
            }
            d["tokens"] = td;
        }

        if (a.Meta is not null)
        {
            var m = a.Meta;
            d["meta"] = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["title"]    = m.Title,
                ["author"]   = m.Author,
                ["subject"]  = m.Subject,
                ["keywords"] = m.Keywords,
                ["creator"]  = m.Creator,
            };
        }

        return new PdfDocument(d, (nodes ?? []).ToList());
    }

    /// <summary>Build a <c>section</c> (page) node.</summary>
    public static SectionNode Section(SectionAttr? attrs = null, SectionContent[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>Wrap layout nodes into a <c>layout</c> block.</summary>
    public static SectionLayout Layout(object? _attrs, Node[]? nodes = null)
        => new((nodes ?? []).ToList(), new Dictionary<string, string>(StringComparer.Ordinal));

    /// <summary>Wrap canvas layer nodes into a <c>canvas</c> block.</summary>
    public static SectionCanvas Canvas(object? _attrs, LayerNode[]? layers = null)
        => new((layers ?? []).ToList(), new Dictionary<string, string>(StringComparer.Ordinal));

    // ── Layout containers ─────────────────────────────────────────────────────

    /// <summary>Build a <c>stack</c> layout node (vertical column).</summary>
    public static ContainerNode Stack(StackAttr? attrs = null, Node[]? nodes = null)
        => Container("stack", attrs, nodes);

    /// <summary>Build a <c>flank</c> layout node (horizontal row).</summary>
    public static ContainerNode Flank(FlankAttr? attrs = null, Node[]? nodes = null)
        => Container("flank", attrs, nodes);

    /// <summary>Build a <c>split</c> layout node (two-column split).</summary>
    public static ContainerNode Split(SplitAttr? attrs = null, Node[]? nodes = null)
        => Container("split", attrs, nodes);

    /// <summary>Build a <c>cluster</c> layout node (wrapping flex row).</summary>
    public static ContainerNode Cluster(ClusterAttr? attrs = null, Node[]? nodes = null)
        => Container("cluster", attrs, nodes);

    /// <summary>Build a <c>grid</c> layout node (multi-column grid).</summary>
    public static ContainerNode Grid(GridAttr? attrs = null, Node[]? nodes = null)
        => Container("grid", attrs, nodes);

    /// <summary>Build a <c>frame</c> layout node (fixed-size container).</summary>
    public static ContainerNode Frame(FrameAttr? attrs = null, Node[]? nodes = null)
        => Container("frame", attrs, nodes);

    /// <summary>Build a <c>link</c> layout node (hyperlink wrapper).</summary>
    public static ContainerNode Link(LinkAttr? attrs = null, Node[]? nodes = null)
        => Container("link", attrs, nodes);

    // ── Table ─────────────────────────────────────────────────────────────────

    /// <summary>Build a <c>table</c> layout node.</summary>
    public static ContainerNode Table(TableAttr? attrs = null, Node[]? nodes = null)
        => Container("table", attrs, nodes);

    /// <summary>Build a <c>thead</c> table header row group.</summary>
    public static ContainerNode Thead(TheadAttr? attrs = null, Node[]? nodes = null)
        => Container("thead", attrs, nodes);

    /// <summary>Build a <c>tr</c> table row.</summary>
    public static ContainerNode Tr(TrAttr? attrs = null, Node[]? nodes = null)
        => Container("tr", attrs, nodes);

    /// <summary>Build a <c>td</c> table cell.</summary>
    public static ContainerNode Td(TdAttr? attrs = null, Node[]? nodes = null)
        => Container("td", attrs, nodes);

    // ── Layout leaves ─────────────────────────────────────────────────────────

    /// <summary>Build a <c>text</c> paragraph node.</summary>
    public static Layout.TextNode Text(Content[]? nodes = null, TextAttr? attrs = null) => new(
        AttrsHelper.Attrs(attrs),
        (nodes ?? []).ToList());

    /// <summary>Wrap a plain string as inline <see cref="Content"/>.</summary>
    public static Content Raw(string raw) => new RawText(raw);

    /// <summary>Build a <c>span</c> inline node.</summary>
    public static SpanNode Span(string[]? nodes = null, SpanAttr? attrs = null) => new(
        AttrsHelper.Attrs(attrs),
        (nodes ?? []).ToList());

    /// <summary>Build a <c>divider</c> horizontal rule node.</summary>
    public static DividerNode Divider(DividerAttr? attrs = null)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build an <c>img</c> image node.</summary>
    public static ImgNode Img(ImgAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>barcode</c> node.</summary>
    public static BarcodeNode Barcode(BarcodeAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>region</c> node.</summary>
    public static RegionNode Region(RegionAttr? attrs = null, Node[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>Build a <c>field</c> form node.</summary>
    public static FieldNode Field(FieldAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    // ── Canvas ────────────────────────────────────────────────────────────────

    /// <summary>Build a <c>canvas-layer</c> node containing canvas primitives.</summary>
    public static LayerNode Layer(LayerAttr? attrs = null, CanvasNode[]? nodes = null)
        => new((nodes ?? []).ToList(), attrs);

    /// <summary>Build a <c>canvas-rect</c> node.</summary>
    public static RectNode Rect(double x, double y, double w, double h, RectStyle? style = null)
        => new(x, y, w, h, style);

    /// <summary>Build a <c>canvas-line</c> node.</summary>
    public static LineNode Line(double x1, double y1, double x2, double y2, LineStyle? style = null)
        => new(x1, y1, x2, y2, style);

    /// <summary>Build a <c>canvas-ellipse</c> node.</summary>
    public static EllipseNode Ellipse(double cx, double cy, double rx, double ry, EllipseStyle? style = null)
        => new(cx, cy, rx, ry, style);

    /// <summary>Build a <c>canvas-circle</c> node (uniform radii convenience form).</summary>
    public static CircleNode Circle(double cx, double cy, double r, EllipseStyle? style = null)
        => new(cx, cy, r, style);

    /// <summary>Build a <c>canvas-path</c> node from an SVG path string.</summary>
    public static PathNode Path(string d, PathStyle? style = null)
        => new(d, style);

    /// <summary>Build a <c>canvas-text</c> node.</summary>
    public static Canvas.TextNode TextAt(double x, double y, string content, TextStyle? style = null, Run[]? runs = null)
        => new(x, y, content, style, runs);

    /// <summary>Build a <c>canvas-image</c> node.</summary>
    public static ImageNode ImgAt(double x, double y, string name, double? w = null, double? h = null)
        => new(x, y, name, w, h);

    // ── Private ───────────────────────────────────────────────────────────────

    private static ContainerNode Container(string type, object? attrs, Node[]? children)
        => new(AttrsHelper.Attrs(attrs), (children ?? []).ToList(), type);
}
