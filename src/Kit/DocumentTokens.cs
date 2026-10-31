namespace Lpdf.Kit;

/// <summary>
/// Design-token overrides applied to the whole document. <c>Gradients</c> maps the name of each gradient to a
/// <see cref="Gradient"/>: its <c>Type</c> (<c>linear</c> or <c>radial</c>), <c>From</c> and <c>To</c>, or
/// <c>Stops</c> as a list of <see cref="GradientStop"/>, and <c>Angle</c> in degrees. A background or a fill uses
/// one as <c>gradient:name</c>. <c>LineHeight</c> is the row of factors, each a number above 0. <c>Aliases</c> maps a name to the slot of the scale it stands for: <c>["caption"] = "xs"</c>
/// lets <c>FontSize = "caption"</c> mean the text size <c>xs</c>, and <c>Gap = "caption"</c> the space <c>xs</c>.
/// <c>Height</c> is the row of heights that <c>Height = "m"</c> names, from <c>xs</c> to <c>xxl</c>.
/// </summary>
public sealed record DocumentTokens(
    Dictionary<string, string>?   Colors = null,
    Dictionary<string, string>?   Space  = null,
    Dictionary<string, string>?   Grid   = null,
    Dictionary<string, string>?   Border = null,
    Dictionary<string, string>?   Radius = null,
    Dictionary<string, string>?   Width  = null,
    Dictionary<string, string>?   TextSize = null,
    Dictionary<string, double>?   LineHeight = null,
    Dictionary<string, Gradient>? Gradients = null,
    Dictionary<string, string>?   Aliases = null,
    Dictionary<string, string>?   Height = null);
