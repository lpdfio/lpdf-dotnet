// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>Declares an ICC color profile that the profile attribute of output names: the profile of the printing press a PDF/X file is made for, which the printer gives out. The SDK reads the file from src, or the profile was loaded on the engine under ref or, with no ref, under its own name. The engine bundles no press profile.</summary>
public sealed record ProfileAttr
{
    /// <summary>The name that the profile attribute of output uses to pick this profile: lowercase letters, digits and -, starting with a letter.</summary>
    public required string Name { get; init; }

    /// <summary>The key the profile was loaded under on the engine, when that is not its name.</summary>
    public string? Ref { get; init; }

    /// <summary>A path the SDK reads the ICC profile from, when the profile was not loaded on the engine.</summary>
    public string? Src { get; init; }
}
