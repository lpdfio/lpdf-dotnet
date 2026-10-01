using System.Globalization;

namespace Lpdf.Canvas;

#pragma warning disable CS1591

/// <summary>A 6-element affine transform matrix <c>[a, b, c, d, e, f]</c>.</summary>
public sealed record Transform(double A, double B, double C, double D, double E, double F)
{
    /// <summary>The <c>matrix(a,b,c,d,e,f)</c> form, which a layer's <c>transform</c> attribute accepts.</summary>
    public override string ToString()
    {
        var values = new[] { A, B, C, D, E, F }.Select(v => v.ToString(CultureInfo.InvariantCulture));
        return $"matrix({string.Join(",", values)})";
    }
}
