using System.Text.Json.Serialization;

namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>
/// Abstract base for the nodes a layer holds.
/// Use <see cref="L"/> to construct instances.
/// </summary>
[JsonConverter(typeof(Lpdf.Shared.CanvasNodeConverter))]
public abstract record CanvasNode
{
    /// <summary>The element name, as the schema spells it (<c>rect</c>, <c>text</c>, <c>layer</c>).</summary>
    public abstract string Type { get; }
}
