using Lpdf.Canvas;
using Lpdf.Kit;
using Lpdf.Layout;

namespace Lpdf;

/// <summary>
/// The type of <see cref="L.NoAttr"/>: "no attributes". It converts to the attribute type of every
/// <see cref="L"/> builder that takes its attributes as an optional argument, so <c>L.Stack(NoAttr, [...])</c>
/// reads the same as it does in the other SDKs.
///
/// <c>img</c>, <c>barcode</c>, <c>region</c> and <c>field</c> need attributes, so there is no conversion to
/// their types: <c>L.Img(NoAttr)</c> does not compile, where it would build a node the engine rejects.
/// </summary>
public sealed class NoAttributes
{
    internal static readonly NoAttributes Instance = new();

    private NoAttributes()
    {
    }

    // One conversion per optional attribute type of an L builder. NoAttributesTests fails when a builder
    // gains an optional attribute type that is missing here.
#pragma warning disable CS1591
    public static implicit operator DocumentAttr?(NoAttributes _) => null;
    public static implicit operator SectionAttr?(NoAttributes _) => null;
    public static implicit operator StackAttr?(NoAttributes _) => null;
    public static implicit operator FlankAttr?(NoAttributes _) => null;
    public static implicit operator SplitAttr?(NoAttributes _) => null;
    public static implicit operator ClusterAttr?(NoAttributes _) => null;
    public static implicit operator GridAttr?(NoAttributes _) => null;
    public static implicit operator FrameAttr?(NoAttributes _) => null;
    public static implicit operator TheadAttr?(NoAttributes _) => null;
    public static implicit operator TrAttr?(NoAttributes _) => null;
    public static implicit operator TdAttr?(NoAttributes _) => null;
    public static implicit operator TextAttr?(NoAttributes _) => null;
    public static implicit operator SpanAttr?(NoAttributes _) => null;
    public static implicit operator DividerAttr?(NoAttributes _) => null;
    public static implicit operator LayerAttr?(NoAttributes _) => null;
#pragma warning restore CS1591
}
