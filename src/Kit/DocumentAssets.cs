using Lpdf.Shared;

namespace Lpdf.Kit;

/// <summary>
/// The fonts and images the document declares, as the <c>assets</c> element of the XML does. A font
/// or image is picked by its <c>name</c>: text sets <c>font</c> to a font's name, an <c>img</c> sets
/// <c>name</c> to an image's.
/// </summary>
public sealed record DocumentAssets(
    IReadOnlyList<FontAttr>?  Fonts  = null,
    IReadOnlyList<ImageAttr>? Images = null)
{
    /// <summary>The assets as the attributes of their elements, in the shape the engine reads.</summary>
    internal Dictionary<string, object?> ToAttrs()
    {
        var declared = new Dictionary<string, object?>(StringComparer.Ordinal);
        if (Fonts is { Count: > 0 })  declared["fonts"]  = Fonts.Select(font => AttrsHelper.Attrs(font)).ToList();
        if (Images is { Count: > 0 }) declared["images"] = Images.Select(image => AttrsHelper.Attrs(image)).ToList();
        return declared;
    }
}
