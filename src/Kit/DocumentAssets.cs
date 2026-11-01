using Lpdf.Shared;

namespace Lpdf.Kit;

/// <summary>
/// The fonts, images and color profiles the document declares, as the <c>assets</c> element of the XML does. A font
/// or image is picked by its <c>name</c>: text sets <c>font</c> to a font's name, an <c>img</c> sets <c>name</c> to an
/// image's, and the output's <c>Profile</c> names a profile.
/// </summary>
/// <param name="Fonts">The fonts text can be set in.</param>
/// <param name="Images">The images an <c>img</c> can show.</param>
/// <param name="Profiles">
/// ICC color profiles: the profile of the printing press a PDF/X document is made for, which the printer gives out.
/// Each is read from its <c>Src</c> through the engine's <c>SrcFallback</c>, or loaded with <c>LoadProfile</c> under
/// its <c>Ref</c> or name.
/// </param>
public sealed record DocumentAssets(
    IReadOnlyList<FontAttr>?    Fonts    = null,
    IReadOnlyList<ImageAttr>?   Images   = null,
    IReadOnlyList<ProfileAttr>? Profiles = null)
{
    /// <summary>The assets as the attributes of their elements, in the shape the engine reads.</summary>
    internal Dictionary<string, object?> ToAttrs()
    {
        var declared = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (Fonts is { Count: > 0 })  declared["fonts"]  = Fonts.Select(font => AttrsHelper.Attrs(font)).ToList();
        if (Images is { Count: > 0 }) declared["images"] = Images.Select(image => AttrsHelper.Attrs(image)).ToList();
        if (Profiles is { Count: > 0 }) declared["profiles"] = Profiles.Select(profile => AttrsHelper.Attrs(profile)).ToList();
        return declared;
    }
}
