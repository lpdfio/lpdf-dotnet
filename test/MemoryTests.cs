using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Layout;
using Lpdf.Shared;
using Wasmtime;
using Xunit;

namespace Lpdf.Tests;

/// <summary>Big documents, the memory cap of a render, how deep a document may nest, and an engine that crashes.</summary>
public class MemoryTests
{
    private const string CapError = "the render needs more memory than its cap of ";
    private const string DepthError = "is nested 101 levels deep, past the limit of 100";

    [Fact]
    public void Bench_max_copied_16_times_renders_its_3008_pages()
    {
        if (!SnapshotHelper.HAS_FIXTURES)
            return; // Fixture files not available outside the monorepo.

        var xml = BenchMaxCopied(16);
        Assert.True(xml.Length > 16_000_000);

        var pdf = L.Engine(new EngineOptions { Timeout = 0 }).Render(xml);

        Assert.Equal(3008, PageCount(pdf));
    }

    [Fact]
    public void A_document_tree_whose_json_is_over_4_mb_renders()
    {
        var line = string.Concat(Enumerable.Repeat(
            "A paragraph of a long report, with enough words in it to fill a line or two of the page. ", 2));
        var sections = Enumerable.Range(0, 1200)
            .Select(s => L.Section(null, [L.Layout(null, Enumerable.Range(0, 20)
                .Select(i => (Node)L.Text(null, [$"{s}.{i} {line}"])).ToArray())]))
            .ToArray();
        var tree = L.Document(null, sections);
        Assert.True(JsonSerializer.Serialize(tree, DocumentJson.Options).Length > 4 * 1_048_576);

        var pdf = L.Engine(new EngineOptions { Timeout = 0 }).Render(tree);

        Assert.True(PageCount(pdf) >= 1200);
    }

    [Fact]
    public void A_render_past_the_cap_stops_with_its_error_and_the_next_renders_right()
    {
        if (!SnapshotHelper.HAS_FIXTURES)
            return; // Fixture files not available outside the monorepo.

        using var engine = L.Engine(new EngineOptions { MaxMemoryMb = 8 }).SetLicenseKey("test-key");

        var error = Assert.Throws<EngineException>(() => engine.Render(Fixture("bench_max")));

        Assert.StartsWith(CapError + "8 MB", error.Message);
        SnapshotHelper.CompareOrUpdate("bench_m", engine.Render(Fixture("bench_m"), new RenderOptions { SkipEncryption = true }));
    }

    [Fact]
    public void The_same_document_renders_under_a_higher_cap()
    {
        if (!SnapshotHelper.HAS_FIXTURES)
            return; // Fixture files not available outside the monorepo.

        var pdf = L.Engine(new EngineOptions { MaxMemoryMb = 64 }).Render(Fixture("bench_max"));

        Assert.Equal(188, PageCount(pdf));
    }

    [Fact]
    public void A_big_data_object_counts_toward_the_cap()
    {
        const string xml = "<lpdf version=\"1\"><document><section><layout><text data-value=\"title\">x</text></layout></section></document></lpdf>";
        var rows = Enumerable.Range(0, 200_000).Select(i => new { name = $"Row {i}", note = "Some text that is bound" });
        var data = new { title = "Report", rows };

        var error = Assert.Throws<EngineException>(
            () => L.Engine(new EngineOptions { MaxMemoryMb = 8 }).Render(xml, new RenderOptions { Data = data }));
        Assert.StartsWith(CapError + "8 MB", error.Message);
        var pdf = L.Engine(new EngineOptions { MaxMemoryMb = 256 }).Render(xml, new RenderOptions { Data = data });
        Assert.Equal(1, PageCount(pdf));
    }

    [Fact]
    public void The_cap_is_1024_mb_when_left_out()
    {
        Assert.Equal(1024, new EngineOptions().MaxMemoryMb);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4097)]
    public void A_cap_below_1_or_above_4096_is_refused(int megabytes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EngineOptions { MaxMemoryMb = megabytes });
    }

    [Fact]
    public void A_document_nested_100_levels_deep_renders_as_xml_and_as_a_tree()
    {
        using var engine = L.Engine();

        Assert.Equal(1, PageCount(engine.Render(NestedXml(100))));
        Assert.Equal(1, PageCount(engine.Render(NestedTree(100))));
    }

    [Fact]
    public void A_document_nested_101_levels_deep_fails_with_the_depth_error_as_xml_and_as_a_tree()
    {
        using var engine = L.Engine();

        var xml = Assert.Throws<EngineException>(() => engine.Render(NestedXml(101)));
        var tree = Assert.Throws<EngineException>(() => engine.Render(NestedTree(101)));

        Assert.Contains(DepthError, xml.Message);
        Assert.StartsWith("<text> at line 1", xml.Message);
        Assert.Contains(DepthError, tree.Message);
        Assert.StartsWith("the node 'text'", tree.Message);
    }

    [Fact]
    public void A_tree_nested_100_levels_deep_reads_the_src_of_the_file_it_carries()
    {
        var terms = Path.GetTempFileName();
        File.WriteAllText(terms, "Terms of sale");
        try
        {
            var document = NestedTree(100, new DocumentAttr(Attachments: [new FileAttr { Name = "terms.txt", Src = terms }]));
            using var engine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });

            var pdf = engine.Render(document);

            Assert.Single(Regex.Matches(Encoding.Latin1.GetString(pdf), "/Type /EmbeddedFile"));
        }
        finally
        {
            File.Delete(terms);
        }
    }

    /// <summary>
    /// A stand-in module that traps in place of the engine, as the engine does when its memory runs out: Rust aborts,
    /// which is WebAssembly's <c>unreachable</c>.
    /// </summary>
    [Fact]
    public void A_crash_throws_the_crash_message_with_the_trap_inside()
    {
        const string trap = """(module (func (export "_start") unreachable))""";
        using var engine = new PdfEngine(new EngineOptions(), WasmRunner.ForModuleText(trap, timeout: 0));

        var error = Assert.Throws<EngineException>(() => engine.Render(NestedXml(10)));

        Assert.Equal(
            "The engine crashed during the render. This is a bug in Lpdf, or the render ran out of memory before its " +
            "cap stopped it: please report it with the document at https://github.com/lpdfio/lpdf/issues.",
            error.Message);
        Assert.Equal(TrapCode.Unreachable, Assert.IsType<TrapException>(error.InnerException).Type);
    }

    /// <summary><c>bench_max.xml</c> with its sections copied: 188 pages and 1 MB of XML for each copy.</summary>
    private static string BenchMaxCopied(int copies)
    {
        var xml = Fixture("bench_max");
        var start = xml.IndexOf("<section", StringComparison.Ordinal);
        var end = xml.LastIndexOf("</section>", StringComparison.Ordinal) + "</section>".Length;
        var sections = new StringBuilder();
        for (var i = 0; i < copies; i++) sections.Append(xml, start, end - start);
        return xml[..start] + sections + xml[end..];
    }

    /// <summary>A document whose columns nest so that its deepest element is <paramref name="depth"/> levels deep.</summary>
    private static string NestedXml(int depth)
    {
        // <lpdf>, <document>, <section>, <layout> and the <text> in the deepest column.
        var columns = depth - 5;
        return "<lpdf version=\"1\"><document><section><layout>" + string.Concat(Enumerable.Repeat("<column>", columns))
            + "<text>deep</text>" + string.Concat(Enumerable.Repeat("</column>", columns))
            + "</layout></section></document></lpdf>";
    }

    /// <summary>The tree of <see cref="NestedXml"/>, with the document's <paramref name="attrs"/>.</summary>
    private static PdfDocument NestedTree(int depth, DocumentAttr? attrs = null)
    {
        Node node = L.Text(null, ["deep"]);
        for (var i = 0; i < depth - 5; i++) node = L.Column(null, [node]);
        return L.Document(attrs, [L.Section(null, [L.Layout(null, [node])])]);
    }

    /// <summary>The number of pages of a PDF: its page objects, which the engine writes uncompressed.</summary>
    private static int PageCount(byte[] pdf)
        => Regex.Count(Encoding.Latin1.GetString(pdf), @"/Type /Page\b(?!s)");

    private static string Fixture(string name)
        => File.ReadAllText(Path.Combine(SnapshotHelper.Fixtures, $"{name}.xml"));
}
