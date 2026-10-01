// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Canvas;

#pragma warning disable CS1591
/// <summary>Groups canvas shapes and sets what applies to all of them: which pages they appear on, opacity, transform and clip. Layers cannot be nested.</summary>
public sealed record LayerAttr
{
    public string? Page { get; init; }
    public string? Opacity { get; init; }
    public string? Transform { get; init; }
    public string? Clip { get; init; }
}
