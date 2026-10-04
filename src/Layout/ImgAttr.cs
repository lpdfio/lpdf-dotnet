// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Layout;

#pragma warning disable CS1591
/// <summary>Attributes of the <c>img</c> element.</summary>
public sealed record ImgAttr
{
    public required string Name { get; init; }
    public string? Height { get; init; }
    public string? Width { get; init; }
    public string? Font { get; init; }
    public string? FontSize { get; init; }
    public string? Gap { get; init; }
    public string? Padding { get; init; }
    public string? Background { get; init; }
    public string? Border { get; init; }
    public string? Radius { get; init; }
    public string? Paginate { get; init; }
    public string? Debug { get; init; }
}
