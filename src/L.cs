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
/// using Lpdf.Kit;
/// using Lpdf.Layout;
/// using static Lpdf.L;
///
/// var doc = L.Document(new DocumentAttr(Size: "a4"), [
///     L.Section(NoAttr, [
///         L.Layout(NoAttr, [
///             L.Text(new TextAttr(Font: "heading"), ["Hello"]),
///         ])
///     ])
/// ]);
/// var bytes = await L.Engine().SetLicenseKey("…").Render(doc);
/// </code>
/// </example>
/// </summary>
public static class L
{
    /// <summary>
    /// No attributes: pass as the <c>attrs</c> argument of any builder that takes its attributes as an
    /// optional argument. Reach it as <c>NoAttr</c> with <c>using static Lpdf.L;</c>, or as <c>L.NoAttr</c>.
    /// </summary>
    public static readonly NoAttributes NoAttr = NoAttributes.Instance;

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
        if (a.Font        is not null) d["font"]        = a.Font;
        if (a.Debug       is not null) d["debug"]       = a.Debug;

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
            if (t.TextSize is not null) td["text-size"] = t.TextSize;
            d["tokens"] = td;
        }

        if (a.Assets is not null) d["assets"] = a.Assets.ToAttrs();

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

    /// <summary>Create a <see cref="DocumentAssets"/> instance (convenience factory).</summary>
    public static DocumentAssets Assets(DocumentAssets attrs)
        => attrs;

    /// <summary>Create a <see cref="DocumentTokens"/> instance (convenience factory).</summary>
    public static DocumentTokens Tokens(DocumentTokens attrs)
        => attrs;

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
    public static ContainerNode Link(LinkAttr attrs, Node[]? nodes = null)
        => Container("link", attrs, nodes);

    // ── Table ─────────────────────────────────────────────────────────────────

    /// <summary>Build a <c>table</c> layout node.</summary>
    public static ContainerNode Table(TableAttr attrs, Node[]? nodes = null)
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

    /// <summary>
    /// Build a <c>text</c> paragraph node. Each item of <paramref name="nodes"/> is a plain string or a
    /// <see cref="SpanNode"/>, and the two can be mixed.
    /// </summary>
    public static Layout.TextNode Text(TextAttr? attrs = null, TextContent[]? nodes = null) => new(
        AttrsHelper.Attrs(attrs),
        (nodes ?? []).Select(item => item.Value).ToList());

    /// <summary>
    /// Wrap a plain string as inline text. A string converts to <see cref="TextContent"/> on its own, so this
    /// is only needed to name a run before it is used.
    /// </summary>
    public static TextContent Raw(string raw) => raw;

    /// <summary>Build a <c>span</c> inline node.</summary>
    public static SpanNode Span(SpanAttr? attrs = null, string[]? nodes = null) => new(
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
    public static RegionNode Region(RegionAttr attrs, Node[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>Build a <c>field</c> form node.</summary>
    public static FieldNode Field(FieldAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    // ── Canvas ────────────────────────────────────────────────────────────────

    /// <summary>Build a <c>layer</c> containing canvas primitives.</summary>
    public static LayerNode Layer(LayerAttr? attrs = null, CanvasNode[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>Build a <c>rect</c> on the canvas.</summary>
    public static RectNode Rect(RectAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>line</c> on the canvas.</summary>
    public static LineNode Line(LineAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build an <c>ellipse</c> on the canvas.</summary>
    public static EllipseNode Ellipse(EllipseAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>circle</c> on the canvas.</summary>
    public static CircleNode Circle(CircleAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>path</c> on the canvas from an SVG path string in <c>D</c>.</summary>
    public static PathNode Path(PathAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>
    /// Build text on the canvas. Each item of <paramref name="nodes"/> is a plain string or a
    /// <see cref="SpanNode"/>, and the two can be mixed.
    /// </summary>
    public static Canvas.TextNode TextAt(CanvasTextAttr attrs, TextContent[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).Select(item => item.Value).ToList());

    /// <summary>Build an <c>img</c> on the canvas.</summary>
    public static ImageNode ImgAt(CanvasImgAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    // ── Private ───────────────────────────────────────────────────────────────

    private static ContainerNode Container(string type, object? attrs, Node[]? children)
        => new(AttrsHelper.Attrs(attrs), (children ?? []).ToList(), type);
}
