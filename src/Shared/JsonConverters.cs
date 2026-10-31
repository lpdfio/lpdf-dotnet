using System.Text.Json;
using System.Text.Json.Serialization;
using Lpdf.Canvas;
using Lpdf.Kit;
using Lpdf.Layout;
using Lpdf.Markdown;

namespace Lpdf.Shared;

// ──────────────────────────────────────────────────────────────────────────────
// NodeConverter — serialises Lpdf.Layout.Node subtypes
// ──────────────────────────────────────────────────────────────────────────────

internal sealed class NodeConverter : JsonConverter<Layout.Node>
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(Layout.Node).IsAssignableFrom(typeToConvert);

    public override Layout.Node Read(ref Utf8JsonReader _, Type __, JsonSerializerOptions ___)
        => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, Layout.Node value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);

        switch (value)
        {
            case ContainerNode c:
                WriteAttrs(writer, c.Attrs);
                WriteLayoutNodes(writer, c.Nodes, options);
                break;
            case Layout.TextNode t:
                WriteAttrs(writer, t.Attrs);
                WriteTextContent(writer, t.Children, options);
                break;
            case CodeNode code:
                WriteAttrs(writer, code.Attrs);
                WriteTextContent(writer, code.Children, options);
                break;
            case SpanNode s:
                WriteAttrs(writer, s.Attrs);
                writer.WriteStartArray("nodes");
                foreach (var str in s.Children) writer.WriteStringValue(str);
                writer.WriteEndArray();
                break;
            case CodeSpanNode codeSpan:
                WriteAttrs(writer, codeSpan.Attrs);
                writer.WriteStartArray("nodes");
                foreach (var str in codeSpan.Children) writer.WriteStringValue(str);
                writer.WriteEndArray();
                break;
            case FnNode fn:
                WriteAttrs(writer, fn.Attrs);
                WriteTextContent(writer, fn.Children, options);
                break;
            case RegionNode r:
                WriteAttrs(writer, r.Attrs);
                WriteLayoutNodes(writer, r.Nodes, options);
                break;
            case DividerNode d:
                WriteAttrs(writer, d.Attrs);
                break;
            case ImgNode img:
                WriteAttrs(writer, img.Attrs);
                break;
            case ShapeNode shape:
                WriteAttrs(writer, shape.Attrs);
                break;
            case BarcodeNode bc:
                WriteAttrs(writer, bc.Attrs);
                break;
            case FieldNode f:
                WriteAttrs(writer, f.Attrs);
                break;
            case BookmarkNode b:
                WriteAttrs(writer, b.Attrs);
                break;
        }

        writer.WriteEndObject();
    }

    internal static void WriteAttrs(Utf8JsonWriter writer, Dictionary<string, object> attrs)
    {
        writer.WriteStartObject("attrs");
        foreach (var (k, v) in attrs) WriteAttr(writer, k, v);
        writer.WriteEndObject();
    }

    /// <summary>
    /// One attribute, in the JSON type of its value: a string, <c>true</c> or <c>false</c>, or a number. The engine
    /// checks it against the type the schema gives the attribute.
    /// </summary>
    internal static void WriteAttr(Utf8JsonWriter writer, string name, object value)
    {
        switch (value)
        {
            case string text: writer.WriteString(name, text); break;
            case bool flag: writer.WriteBoolean(name, flag); break;
            case int whole: writer.WriteNumber(name, whole); break;
            case double number: writer.WriteNumber(name, number); break;
            case LineHeight lineHeight: WriteAttr(writer, name, lineHeight.Value); break;
            default:
                writer.WritePropertyName(name);
                JsonSerializer.Serialize(writer, value, value.GetType());
                break;
        }
    }

    internal static void WriteLayoutNodes(Utf8JsonWriter writer, List<Layout.Node> nodes, JsonSerializerOptions options)
    {
        writer.WriteStartArray("nodes");
        foreach (var child in nodes)
            JsonSerializer.Serialize(writer, child, options);
        writer.WriteEndArray();
    }

    internal static void WriteTextContent(Utf8JsonWriter writer, List<Content> children, JsonSerializerOptions options)
    {
        writer.WriteStartArray("nodes");
        foreach (var item in children)
        {
            if (item is RawText raw)
                writer.WriteStringValue(raw.Value);
            else if (item is SpanNode span)
                JsonSerializer.Serialize(writer, (Layout.Node)span, options);
            else if (item is CodeSpanNode codeSpan)
                JsonSerializer.Serialize(writer, (Layout.Node)codeSpan, options);
            else if (item is FnNode fn)
                JsonSerializer.Serialize(writer, (Layout.Node)fn, options);
        }
        writer.WriteEndArray();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// CanvasNodeConverter — serialises Lpdf.Canvas.CanvasNode subtypes
// ──────────────────────────────────────────────────────────────────────────────

internal sealed class CanvasNodeConverter : JsonConverter<CanvasNode>
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(CanvasNode).IsAssignableFrom(typeToConvert);

    public override CanvasNode Read(ref Utf8JsonReader _, Type __, JsonSerializerOptions ___)
        => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, CanvasNode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);

        switch (value)
        {
            case LayerNode layer:
                NodeConverter.WriteAttrs(writer, layer.Attrs);
                writer.WriteStartArray("nodes");
                foreach (var node in layer.Nodes)
                    JsonSerializer.Serialize(writer, node, options);
                writer.WriteEndArray();
                break;
            case TextAtNode text:
                NodeConverter.WriteAttrs(writer, text.Attrs);
                NodeConverter.WriteTextContent(writer, text.Children, options);
                break;
            case RectNode rect:
                NodeConverter.WriteAttrs(writer, rect.Attrs);
                break;
            case LineNode line:
                NodeConverter.WriteAttrs(writer, line.Attrs);
                break;
            case EllipseNode ellipse:
                NodeConverter.WriteAttrs(writer, ellipse.Attrs);
                break;
            case CircleNode circle:
                NodeConverter.WriteAttrs(writer, circle.Attrs);
                break;
            case PathNode path:
                NodeConverter.WriteAttrs(writer, path.Attrs);
                break;
            case ImgAtNode img:
                NodeConverter.WriteAttrs(writer, img.Attrs);
                break;
            case BarcodeAtNode barcode:
                NodeConverter.WriteAttrs(writer, barcode.Attrs);
                break;
        }

        writer.WriteEndObject();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// SectionContentConverter — serialises SectionBookmark, SectionLayer and SectionLayout
// ──────────────────────────────────────────────────────────────────────────────

internal sealed class SectionContentConverter : JsonConverter<SectionContent>
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(SectionContent).IsAssignableFrom(typeToConvert);

    public override SectionContent Read(ref Utf8JsonReader _, Type __, JsonSerializerOptions ___)
        => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, SectionContent value, JsonSerializerOptions options)
    {
        if (value is SectionLayer layer)
        {
            JsonSerializer.Serialize<CanvasNode>(writer, layer.Layer, options);
            return;
        }
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        if (value is SectionBookmark bookmark)
        {
            NodeConverter.WriteAttrs(writer, bookmark.Attrs);
            writer.WriteEndObject();
            return;
        }
        if (value is SectionMarkdown markdown)
        {
            NodeConverter.WriteAttrs(writer, markdown.Attrs);
            writer.WriteStartArray("nodes");
            foreach (var part in markdown.Nodes)
                WriteMarkdownPart(writer, part);
            writer.WriteEndArray();
            writer.WriteEndObject();
            return;
        }

        writer.WriteStartObject("attrs");
        if (value is SectionLayout sl)
        {
            foreach (var (k, v) in sl.Attrs) NodeConverter.WriteAttr(writer, k, v);
        }
        writer.WriteEndObject();

        writer.WriteStartArray("nodes");
        if (value is SectionLayout layout)
        {
            foreach (var node in layout.Nodes)
                JsonSerializer.Serialize(writer, node, options);
        }
        writer.WriteEndArray();

        writer.WriteEndObject();
    }

    /// <summary>The styles of a markdown, with a rule for each of their children, or its content, as strings.</summary>
    private static void WriteMarkdownPart(Utf8JsonWriter writer, MarkdownPart part)
    {
        writer.WriteStartObject();
        writer.WriteString("type", part.Type);
        switch (part)
        {
            case MarkdownStylesNode styles:
                NodeConverter.WriteAttrs(writer, styles.Attrs);
                writer.WriteStartArray("nodes");
                foreach (var rule in styles.Rules)
                {
                    writer.WriteStartObject();
                    writer.WriteString("type", rule.RuleType);
                    NodeConverter.WriteAttrs(writer, rule.Attrs);
                    writer.WriteEndObject();
                }
                writer.WriteEndArray();
                break;
            case MarkdownContentNode content:
                NodeConverter.WriteAttrs(writer, new Dictionary<string, object>());
                writer.WriteStartArray("nodes");
                foreach (var text in content.Text) writer.WriteStringValue(text);
                writer.WriteEndArray();
                break;
        }
        writer.WriteEndObject();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// SectionNodeConverter — serialises Kit.SectionNode
// ──────────────────────────────────────────────────────────────────────────────

internal sealed class SectionNodeConverter : JsonConverter<SectionNode>
{
    public override SectionNode Read(ref Utf8JsonReader _, Type __, JsonSerializerOptions ___)
        => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, SectionNode value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        NodeConverter.WriteAttrs(writer, value.Attrs);

        writer.WriteStartArray("nodes");
        foreach (var content in value.Nodes)
            JsonSerializer.Serialize(writer, content, options);
        writer.WriteEndArray();

        writer.WriteEndObject();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// DocumentJson — shared serialisation options for Document → WASM JSON
// ──────────────────────────────────────────────────────────────────────────────

internal static class DocumentJson
{
    internal static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters =
        {
            new NodeConverter(),
            new CanvasNodeConverter(),
            new SectionContentConverter(),
            new SectionNodeConverter(),
        },
    };
}
