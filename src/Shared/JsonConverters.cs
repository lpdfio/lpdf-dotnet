using System.Text.Json;
using System.Text.Json.Serialization;
using Lpdf.Canvas;
using Lpdf.Kit;
using Lpdf.Layout;

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
            case SpanNode s:
                WriteAttrs(writer, s.Attrs);
                writer.WriteStartArray("nodes");
                foreach (var str in s.Children) writer.WriteStringValue(str);
                writer.WriteEndArray();
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
            case BarcodeNode bc:
                WriteAttrs(writer, bc.Attrs);
                break;
            case FieldNode f:
                WriteAttrs(writer, f.Attrs);
                break;
        }

        writer.WriteEndObject();
    }

    internal static void WriteAttrs(Utf8JsonWriter writer, Dictionary<string, string> attrs)
    {
        writer.WriteStartObject("attrs");
        foreach (var (k, v) in attrs) writer.WriteString(k, v);
        writer.WriteEndObject();
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
            case Canvas.TextNode text:
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
            case ImageNode img:
                NodeConverter.WriteAttrs(writer, img.Attrs);
                break;
        }

        writer.WriteEndObject();
    }
}

// ──────────────────────────────────────────────────────────────────────────────
// SectionContentConverter — serialises SectionLayout and SectionCanvas
// ──────────────────────────────────────────────────────────────────────────────

internal sealed class SectionContentConverter : JsonConverter<SectionContent>
{
    public override bool CanConvert(Type typeToConvert)
        => typeof(SectionContent).IsAssignableFrom(typeToConvert);

    public override SectionContent Read(ref Utf8JsonReader _, Type __, JsonSerializerOptions ___)
        => throw new NotSupportedException();

    public override void Write(Utf8JsonWriter writer, SectionContent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        writer.WriteStartObject("attrs");
        if (value is SectionLayout sl)
        {
            // Write extra attrs if any were set (currently attrs dict is empty by convention)
            foreach (var (k, v) in sl.Attrs) writer.WriteString(k, v);
        }
        else if (value is SectionCanvas sc)
        {
            foreach (var (k, v) in sc.Attrs) writer.WriteString(k, v);
        }
        writer.WriteEndObject();

        writer.WriteStartArray("nodes");
        if (value is SectionLayout layout)
        {
            foreach (var node in layout.Nodes)
                JsonSerializer.Serialize(writer, node, options);
        }
        else if (value is SectionCanvas canvas)
        {
            foreach (var layer in canvas.Layers)
                JsonSerializer.Serialize(writer, layer, options);
        }
        writer.WriteEndArray();

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
