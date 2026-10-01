// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>Declares a font that text can be set in by name, with the font attribute. A built-in PDF font is named by core; any other font is a file the SDK reads from src, or one loaded on the engine under ref or, with no ref, under the font's own name.</summary>
public sealed record FontAttr
{
    /// <summary>The name that the font attribute of a text uses to pick this font: lowercase letters, digits and -, starting with a letter.</summary>
    public required string Name { get; init; }

    /// <summary>A built-in PDF font, which needs no file.</summary>
    public string? Core { get; init; }

    /// <summary>The key the font was loaded under on the engine, when that is not its name.</summary>
    public string? Ref { get; init; }

    /// <summary>A path the SDK reads the font file from, when the font was not loaded on the engine.</summary>
    public string? Src { get; init; }
}
