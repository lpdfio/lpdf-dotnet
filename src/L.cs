using Lpdf.Canvas;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Layout;
using Lpdf.Markdown;
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
        if (a.Background  is not null) d["background"]  = a.Background;
        if (a.Font        is not null) d["font"]        = a.Font;
        if (a.Debug       is not null) d["debug"]       = a.Debug;
        if (a.FootnoteCounter is not null) d["footnote-counter"] = a.FootnoteCounter;

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
            if (t.LineHeight is not null) td["line-height"] = t.LineHeight;
            if (t.Gradients is not null) td["gradients"] = t.Gradients;
            if (t.Aliases is not null) td["aliases"] = t.Aliases;
            if (t.Height is not null) td["height"] = t.Height;
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

        if (a.Output is not null)
        {
            d["output"] = new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["encrypt"] = a.Output.Encrypt,
                ["allow"]   = a.Output.Allow,
                ["pdfa"]    = a.Output.PdfA,
            };
        }

        return new PdfDocument(d, (nodes ?? []).ToList());
    }

    /// <summary>
    /// Build a <c>section</c> (page) node. Its content is a bookmark, then layers, then at most one layout, then
    /// layers: the layers before the layout paint under it, and the layers after it paint over it.
    /// </summary>
    public static SectionNode Section(SectionAttr? attrs = null, SectionContent[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>Wrap layout nodes into a <c>layout</c> block, which keeps them inside its margin.</summary>
    public static SectionLayout Layout(LayoutAttr? attrs = null, Node[]? nodes = null)
        => new((nodes ?? []).ToList(), AttrsHelper.Attrs(attrs));

    /// <summary>
    /// Build a <c>markdown</c> block, which takes the place of a layout in a section and holds only Markdown: the
    /// <see cref="LMarkdown.Styles"/> that say how each part looks, which are optional, and one
    /// <see cref="LMarkdown.Content"/> with the text. What Markdown can't say, such as the page, a header, a
    /// footer or a cover, stays in the section around it: layers paint a header and a footer in the margin it leaves.
    /// </summary>
    public static SectionMarkdown Markdown(MarkdownAttr? attrs = null, MarkdownPart[]? nodes = null)
        => new((nodes ?? []).ToList(), AttrsHelper.Attrs(attrs));

    /// <summary>Create a <see cref="DocumentAssets"/> instance (convenience factory).</summary>
    public static DocumentAssets Assets(DocumentAssets attrs)
        => attrs;

    /// <summary>Create a <see cref="DocumentTokens"/> instance (convenience factory).</summary>
    public static DocumentTokens Tokens(DocumentTokens attrs)
        => attrs;

    // ── Layout containers ─────────────────────────────────────────────────────

    /// <summary>Build a <c>column</c> layout node (children top to bottom).</summary>
    public static ContainerNode Column(ColumnAttr? attrs = null, Node[]? nodes = null)
        => Container("column", attrs, nodes);

    /// <summary>Build a <c>row</c> layout node (children side by side, wrapping).</summary>
    public static ContainerNode Row(RowAttr? attrs = null, Node[]? nodes = null)
        => Container("row", attrs, nodes);

    /// <summary>Build a <c>grid</c> layout node (multi-column grid).</summary>
    public static ContainerNode Grid(GridAttr? attrs = null, Node[]? nodes = null)
        => Container("grid", attrs, nodes);

    /// <summary>
    /// Build a <c>list</c> layout node: a list of items, which are <see cref="Li"/> nodes. A bullet list marks each
    /// item with a shape; with <c>Type = "number"</c> the engine numbers them, in the <c>Format</c> given, and the
    /// numbers follow the items that <c>data-source</c> makes.
    /// </summary>
    public static ContainerNode List(ListAttr? attrs = null, Node[]? nodes = null)
        => Container("list", attrs, nodes);

    /// <summary>
    /// Build an <c>li</c>, an item of a <see cref="List"/>. Its words go in a <see cref="Text"/>; a <see cref="List"/>
    /// inside it is a list under the item.
    /// </summary>
    public static ContainerNode Li(LiAttr? attrs = null, Node[]? nodes = null)
        => Container("li", attrs, nodes);

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
    /// Build a <c>text</c> paragraph node. Each item of <paramref name="nodes"/> is a plain string, a
    /// <see cref="SpanNode"/>, a <see cref="CodeSpanNode"/> or a <see cref="FnNode"/>, and they can be mixed.
    /// </summary>
    public static Layout.TextNode Text(TextAttr? attrs = null, TextContent[]? nodes = null) => new(
        AttrsHelper.Attrs(attrs),
        (nodes ?? []).Select(item => item.Value).ToList());

    /// <summary>
    /// Build a <c>code</c> node: code in a box that fills the width and grows with its lines. The strings of
    /// <paramref name="nodes"/> are kept as they are written: every space, and a new line wherever there is a
    /// <c>\n</c>. A line longer than the box wraps. The items are strings and <see cref="SpanNode"/>s, which color
    /// parts of the code.
    /// </summary>
    public static CodeNode Code(CodeAttr? attrs = null, TextContent[]? nodes = null) => new(
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

    /// <summary>Build a <c>code-span</c> inline node: a run of code in a <c>text</c>, in the code font on a tint.</summary>
    public static CodeSpanNode CodeSpan(CodeSpanAttr? attrs = null, string[]? nodes = null) => new(
        AttrsHelper.Attrs(attrs),
        (nodes ?? []).ToList());

    /// <summary>
    /// Build an <c>fn</c> footnote, for the nodes of a <c>text</c>. The engine numbers it and puts a small raised
    /// number at its place in the text; its words go at the bottom of the page where that number lands. It has
    /// no attributes, so pass <see cref="NoAttr"/>. Its words are strings and spans.
    /// </summary>
    public static FnNode Fn(NoAttributes? attrs = null, TextContent[]? nodes = null) => new(
        [],
        (nodes ?? []).Select(item => item.Value).ToList());

    /// <summary>Build a <c>divider</c> horizontal rule node.</summary>
    public static DividerNode Divider(DividerAttr? attrs = null)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build an <c>img</c> image node.</summary>
    public static ImgNode Img(ImgAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>shape</c> node: a rect, circle or ellipse that takes its box in the layout, as an image does.</summary>
    public static ShapeNode Shape(ShapeAttr attrs)
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

    /// <summary>
    /// Build a <c>bookmark</c>: an entry of the PDF's outline and a target for <c>href="#id"</c> links. It marks
    /// the node that follows it; in a section's content it marks the section's first page.
    /// </summary>
    public static BookmarkNode Bookmark(BookmarkAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    // ── Layers ────────────────────────────────────────────────────────────────

    /// <summary>Build a <c>layer</c>, which goes directly in a section, holding what it draws at fixed places.</summary>
    public static LayerNode Layer(LayerAttr? attrs = null, CanvasNode[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).ToList());

    /// <summary>Build a <c>rect</c> in a layer.</summary>
    public static RectNode Rect(RectAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>line</c> in a layer.</summary>
    public static LineNode Line(LineAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build an <c>ellipse</c> in a layer.</summary>
    public static EllipseNode Ellipse(EllipseAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>circle</c> in a layer.</summary>
    public static CircleNode Circle(CircleAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>path</c> in a layer from an SVG path string in <c>D</c>.</summary>
    public static PathNode Path(PathAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>
    /// Build a <c>text-at</c>, the text of the layout put at a fixed place. Each item of <paramref name="nodes"/>
    /// is a plain string, a <see cref="SpanNode"/> or a <see cref="CodeSpanNode"/>, and they can be mixed.
    /// </summary>
    public static TextAtNode TextAt(TextAtAttr attrs, TextContent[]? nodes = null)
        => new(AttrsHelper.Attrs(attrs), (nodes ?? []).Select(item => item.Value).ToList());

    /// <summary>Build an <c>img-at</c>, an image at a fixed place. It needs <c>Width</c> or <c>Height</c>.</summary>
    public static ImgAtNode ImgAt(ImgAtAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    /// <summary>Build a <c>barcode-at</c>, a barcode at a fixed place.</summary>
    public static BarcodeAtNode BarcodeAt(BarcodeAtAttr attrs)
        => new(AttrsHelper.Attrs(attrs));

    // ── Private ───────────────────────────────────────────────────────────────

    private static ContainerNode Container(string type, object? attrs, Node[]? children)
        => new(AttrsHelper.Attrs(attrs), (children ?? []).ToList(), type);
}
