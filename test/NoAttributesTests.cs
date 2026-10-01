using System.Reflection;
using Lpdf;
using Lpdf.Layout;
using Xunit;

namespace Lpdf.Tests;

public class NoAttributesTests
{
    [Fact]
    public void NoAttr_converts_to_every_optional_attribute_parameter_of_the_builder()
    {
        var convertible = typeof(NoAttributes)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == "op_Implicit")
            .Select(method => method.ReturnType)
            .ToHashSet();

        var missing = typeof(L)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .SelectMany(method => method.GetParameters())
            .Where(parameter => parameter.IsOptional && parameter.ParameterType.Name.EndsWith("Attr", StringComparison.Ordinal))
            .Select(parameter => parameter.ParameterType)
            .Where(type => !convertible.Contains(type))
            .Select(type => type.Name)
            .Distinct()
            .ToList();

        Assert.Empty(missing);
    }

    [Fact]
    public void NoAttr_does_not_convert_to_the_attribute_types_a_builder_requires()
    {
        var convertible = typeof(NoAttributes)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.Name == "op_Implicit")
            .Select(method => method.ReturnType)
            .ToHashSet();

        Assert.DoesNotContain(typeof(ImgAttr), convertible);
        Assert.DoesNotContain(typeof(BarcodeAttr), convertible);
        Assert.DoesNotContain(typeof(RegionAttr), convertible);
        Assert.DoesNotContain(typeof(FieldAttr), convertible);
    }

    [Fact]
    public void NoAttr_builds_the_same_container_as_null()
    {
        var withNoAttr = L.Stack(L.NoAttr, [L.Text(L.NoAttr, ["x"])]);
        var withNull = L.Stack(null, [L.Text(null, ["x"])]);

        Assert.Empty(withNoAttr.Attrs);
        Assert.Equal(withNull.Attrs, withNoAttr.Attrs);
    }

    [Fact]
    public void NoAttr_builds_the_same_section_as_null()
    {
        var withNoAttr = L.Section(L.NoAttr, [L.Layout(L.NoAttr, [L.Divider(L.NoAttr)])]);
        var withNull = L.Section(null, [L.Layout(null, [L.Divider(null)])]);

        Assert.Empty(withNoAttr.Attrs);
        Assert.Equal(withNull.Attrs, withNoAttr.Attrs);
        Assert.Equal(withNull.Nodes.Count, withNoAttr.Nodes.Count);
    }
}
