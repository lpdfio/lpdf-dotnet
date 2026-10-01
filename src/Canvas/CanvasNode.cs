using System.Text.Json.Serialization;

namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>
/// Abstract base for all canvas node types.
/// Use <see cref="L"/> to construct instances.
/// </summary>
[JsonConverter(typeof(Lpdf.Shared.CanvasNodeConverter))]
public abstract record CanvasNode
{
    /// <summary>The canvas element name, as the schema spells it (<c>rect</c>, <c>text</c>, <c>layer</c>).</summary>
    public abstract string Type { get; }
}
