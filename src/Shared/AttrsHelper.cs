using System.Collections.Concurrent;
using System.Reflection;

namespace Lpdf.Shared;

#pragma warning disable CS1591

internal static class AttrsHelper
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _propCache = new();

    /// <summary>
    /// Reflect over any options record and convert its non-null properties to a kebab-case attribute dictionary.
    /// Each value keeps the type the schema gives the attribute: a <c>string</c>, a <c>bool</c>, an <c>int</c> or a
    /// <c>double</c>, and a <see cref="LineHeight"/> as its name or its factor. A property of any other type, such
    /// as the stops of a gradient, is left out.
    /// </summary>
    internal static Dictionary<string, object> Attrs(object? options)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        if (options is null) return result;

        var props = _propCache.GetOrAdd(
            options.GetType(),
            t => t.GetProperties(BindingFlags.Public | BindingFlags.Instance));
        foreach (var prop in props)
        {
            object? value = prop.GetValue(options) switch
            {
                LineHeight lineHeight => lineHeight.Value,
                var scalar and (string or bool or int or double) => scalar,
                _ => null,
            };
            if (value is not null)
                result[PascalToKebab(prop.Name)] = value;
        }
        return result;
    }

    /// <summary>PascalCase → kebab-case (e.g. <c>FontSize</c> → <c>font-size</c>).</summary>
    internal static string PascalToKebab(string name)
    {
        var sb = new System.Text.StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c) && i > 0) sb.Append('-');
            sb.Append(char.ToLowerInvariant(c));
        }
        return sb.ToString();
    }
}
