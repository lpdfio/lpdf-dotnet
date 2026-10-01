using Lpdf;
using Lpdf.Kit;
using Lpdf.Layout;
using Xunit;

namespace Lpdf.Tests;

public class TextContentTests
{
    [Fact]
    public void Text_takes_attributes_first_and_content_second()
    {
        var node = L.Text(new TextAttr(Bold: "true"), ["Hello"]);

        Assert.Equal("true", node.Attrs["bold"]);
        Assert.Single(node.Children);
    }

    [Fact]
    public void Text_accepts_plain_strings()
    {
        var node = L.Text(L.NoAttr, ["Hello", "World"]);

        Assert.Equal(2, node.Children.Count);
        Assert.All(node.Children, child => Assert.False(child is SpanNode));
    }

    [Fact]
    public void Text_accepts_strings_and_spans_in_one_list()
    {
        var node = L.Text(L.NoAttr, [
            "Total: ",
            L.Span(new SpanAttr(Bold: "true"), ["$100"]),
            " due",
        ]);

        Assert.Equal(3, node.Children.Count);
        Assert.False(node.Children[0] is SpanNode);
        var span = Assert.IsType<SpanNode>(node.Children[1]);
        Assert.Equal("true", span.Attrs["bold"]);
        Assert.False(node.Children[2] is SpanNode);
    }

    [Fact]
    public void Text_without_arguments_has_no_attributes_and_no_children()
    {
        var node = L.Text();

        Assert.Empty(node.Attrs);
        Assert.Empty(node.Children);
    }

    [Fact]
    public void Raw_builds_a_run_that_Text_accepts()
    {
        var run = L.Raw("Hello");
        var node = L.Text(L.NoAttr, [run]);

        Assert.Single(node.Children);
        Assert.False(node.Children[0] is SpanNode);
    }

    [Fact]
    public void Text_reads_a_default_content_item_as_empty_text()
    {
        var node = L.Text(L.NoAttr, [default(TextContent)]);

        Assert.Single(node.Children);
        Assert.False(node.Children[0] is SpanNode);
    }

    [Fact]
    public void Span_takes_attributes_first_and_content_second()
    {
        var span = L.Span(new SpanAttr(Color: "primary"), ["$100"]);

        Assert.Equal("primary", span.Attrs["color"]);
        Assert.Equal(["$100"], span.Children);
    }

    [Fact]
    public async Task Text_with_plain_strings_renders_to_a_pdf()
    {
        var doc = L.Document(new DocumentAttr(Size: "a4"), [
            L.Section(L.NoAttr, [
                L.Layout(L.NoAttr, [
                    L.Text(new TextAttr(FontSize: "14pt"), ["Total: ", L.Span(new SpanAttr(Bold: "true"), ["$100"])]),
                ]),
            ]),
        ]);

        var xml = await L.ToXml(doc);
        using var engine = L.Engine().SetLicenseKey("test-key");
        var pdf = await engine.Render(xml);

        Assert.StartsWith("%PDF", System.Text.Encoding.ASCII.GetString(pdf, 0, 4));
    }
}
