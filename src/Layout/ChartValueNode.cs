namespace Lpdf.Layout;

/// <summary>
/// A value of a category, as plain text: a number, or nothing for no value. Produced from <see cref="LChart.Value"/>.
/// </summary>
public sealed record ChartValueNode(
    List<string> Text)
{
    /// <summary>The element type name emitted to the wire format.</summary>
    public string Type => "value";
}
