// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>Declares an image that an img refers to by name. The SDK reads the file from src, or the image was loaded on the engine under ref or, with no ref, under its own name.</summary>
public sealed record ImageAttr
{
    /// <summary>The name that the name attribute of an img uses to pick this image: lowercase letters, digits and -, starting with a letter.</summary>
    public required string Name { get; init; }

    /// <summary>The key the image was loaded under on the engine, when that is not its name.</summary>
    public string? Ref { get; init; }

    /// <summary>A path the SDK reads the image file from, when the image was not loaded on the engine.</summary>
    public string? Src { get; init; }
}
