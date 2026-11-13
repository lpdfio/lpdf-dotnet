namespace Lpdf.Kit;

/// <summary>
/// Design-token overrides applied to the whole document. <c>Gradients</c> maps the name of each gradient to a
/// <see cref="Gradient"/>: its <c>Type</c> (<c>linear</c> or <c>radial</c>), <c>From</c> and <c>To</c>, or
/// <c>Stops</c> as a list of <see cref="GradientStop"/>, and <c>Angle</c> in degrees. A background or a fill uses
/// one as <c>gradient:name</c>. <c>LineHeight</c> is the row of factors, each a number above 0. <c>Aliases</c> maps a name to the slot of the scale it stands for: <c>["caption"] = "xs"</c>
/// lets <c>FontSize = "caption"</c> mean the text size <c>xs</c>, and <c>Gap = "caption"</c> the space <c>xs</c>.
/// <c>Height</c> is the row of heights that <c>Height = "m"</c> names, from <c>xs</c> to <c>xxl</c>.
/// <c>Rulings</c> maps the name of each ruling to a <see cref="Ruling"/>: its <c>Lines</c>, a list of
/// <see cref="RulingLines"/> each cut by a step or a count in x, in y or both, and its <c>Origin</c>, <c>Edges</c> and
/// <c>Ocg</c>. A section draws one on every page with <c>Ruling = "name"</c>.
/// <c>Spots</c> maps the name of each spot color to a <see cref="Spot"/>: its <c>Ink</c>, the ink's name as the printer
/// knows it, its <c>Value</c>, the color that stands in for it on a screen or a proof, and whether it <c>Overprint</c>s.
/// A color takes the name, or a tint of the ink after a space: <c>Background = "brand 30%"</c>.
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
    Dictionary<string, string>?   Height = null,
    Dictionary<string, Ruling>?   Rulings = null,
    Dictionary<string, Spot>?     Spots = null);
