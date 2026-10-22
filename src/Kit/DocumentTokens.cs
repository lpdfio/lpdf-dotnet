namespace Lpdf.Kit;

/// <summary>
/// Design-token overrides applied to the whole document. <c>Gradients</c> maps the name of each gradient to its
/// attributes: <c>type</c> (<c>linear</c> or <c>radial</c>), <c>from</c> and <c>to</c>, or <c>stops</c> as a
/// list of <c>offset</c> and <c>color</c>, and <c>angle</c> in degrees. A background or a fill uses one as
/// <c>gradient:name</c>. <c>Aliases</c> maps a name to the slot of the scale it stands for: <c>["caption"] = "xs"</c>
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
    Dictionary<string, string>?   LineHeight = null,
    Dictionary<string, Dictionary<string, object>>? Gradients = null,
    Dictionary<string, string>?   Aliases = null,
    Dictionary<string, string>?   Height = null);
