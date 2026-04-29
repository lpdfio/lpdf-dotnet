using System.Text.Json.Serialization;

namespace Lpdf.Kit;

/// <summary>Root document node — passed to <see cref="PdfEngine.Render(PdfDocument, Lpdf.Engine.RenderOptions?)"/>.</summary>
public sealed record PdfDocument(
    [property: JsonPropertyName("attrs")] Dictionary<string, object?> Attrs,
    [property: JsonPropertyName("nodes")] List<SectionNode>           Nodes)
{
    /// <summary>Schema version; always <c>1</c>.</summary>
    [JsonPropertyName("version")] public int    Version { get; } = 1;
    /// <summary>Node type discriminator; always <c>"document"</c>.</summary>
    [JsonPropertyName("type")]    public string Type    { get; } = "document";
}
