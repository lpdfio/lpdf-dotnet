namespace Lpdf.Engine;

/// <summary>
/// One stamp for <see cref="PdfEngine.Stamp"/>: a PNG, the pages it goes on, and the box it is drawn in on each. The box
/// is placed as a layer element is, and the image is fitted in it without distortion.
/// </summary>
public sealed record StampAttr
{
    /// <summary>The PNG's bytes. An array given to several stamps is embedded in the PDF once.</summary>
    public required byte[] Image { get; init; }

    /// <summary>
    /// The pages: a page scope, such as <c>3</c>, <c>1-2</c>, <c>last</c>, <c>each</c>, <c>odd</c> or <c>1,3,5-last</c>. A
    /// page the PDF doesn't have is an error.
    /// </summary>
    public required string Page { get; init; }

    /// <summary>
    /// Where the box goes across, in <c>pt</c>, <c>mm</c> or <c>in</c>: from the page's left edge as a viewer shows it,
    /// or with <see cref="Anchor"/>, a signed offset from that point of the page. Required without an anchor.
    /// </summary>
    public string? X { get; init; }

    /// <summary>
    /// Where the box goes down, in <c>pt</c>, <c>mm</c> or <c>in</c>: from the page's top edge as a viewer shows it, or
    /// with <see cref="Anchor"/>, a signed offset from that point of the page. Required without an anchor.
    /// </summary>
    public string? Y { get; init; }

    /// <summary>
    /// The point of the page the box is placed from, and the point of the box that goes there: <c>top-left</c>,
    /// <c>top-center</c>, <c>top-right</c>, <c>center-left</c>, <c>center</c>, <c>center-right</c>, <c>bottom-left</c>,
    /// <c>bottom-center</c> or <c>bottom-right</c>.
    /// </summary>
    public string? Anchor { get; init; }

    /// <summary>The width of the box, in <c>pt</c>, <c>mm</c> or <c>in</c>.</summary>
    public required string Width { get; init; }

    /// <summary>The height of the box, in <c>pt</c>, <c>mm</c> or <c>in</c>.</summary>
    public required string Height { get; init; }

    /// <summary>Where the image sits across the box, in the room it leaves: <c>start</c>, <c>center</c> (the default) or <c>end</c>.</summary>
    public string? Align { get; init; }

    /// <summary>Where the image sits down the box, in the room it leaves: <c>start</c>, <c>center</c> (the default) or <c>end</c>.</summary>
    public string? Justify { get; init; }

    /// <summary>
    /// How the image is sized in the box, keeping its proportions: <c>fit</c> (the default) shrinks an image larger
    /// than the box and keeps a smaller one at its natural size, from the resolution the PNG records, or a pixel to a
    /// point; <c>fill</c> takes the largest size that fits.
    /// </summary>
    public string? Scale { get; init; }

    /// <summary>How many times its natural size <c>fit</c> may grow an image, never past the box: 1, the default, or more.</summary>
    public double? MaxGrow { get; init; }
}
