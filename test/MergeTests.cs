using System.Text;
using System.Text.RegularExpressions;
using Lpdf.Engine;
using Xunit;

namespace Lpdf.Tests;

/// <summary>Merge: existing PDFs drawn into one PDF made for printing, through the engine's <c>merge</c> request.</summary>
public class MergeTests
{
    /// <summary>A PDF of <paramref name="pages"/> pages, rendered by the engine with the <c>output</c> and the options given.</summary>
    private static byte[] Pdf(int pages = 1, string output = "", RenderOptions? options = null)
    {
        var sections = string.Concat(Enumerable.Range(1, pages).Select(i => $"<section><layout><text>Page {i}</text></layout></section>"));
        using var engine = L.Engine();
        return engine.Render($"<lpdf version=\"1\"><document>{output}{sections}</document></lpdf>", options);
    }

    /// <summary>The text of a merged PDF, which writes its page objects uncompressed.</summary>
    private static string Text(byte[] pdf) => Encoding.Latin1.GetString(pdf);

    /// <summary>How many pages a merged PDF has: its page objects.</summary>
    private static int PageCount(byte[] pdf) => Regex.Matches(Text(pdf), @"/Type\s*/Page(?![s\w])").Count;

    [Fact]
    public void A_merge_returns_one_pdf_with_the_pages_of_the_sources()
    {
        using var engine = L.Engine();
        var output = engine.Merge([new MergeSource { Name = "a.pdf", Pdf = Pdf(2) }, new MergeSource { Name = "b.pdf", Pdf = Pdf(1) }]);
        Assert.StartsWith("%PDF-", Text(output));
        Assert.Equal(3, PageCount(output));
    }

    [Fact]
    public void Double_sided_adds_a_blank_side_after_a_document_with_an_odd_number_of_pages_and_none_after_the_last()
    {
        MergeSource[] sources = [
            new() { Name = "odd.pdf", Pdf = Pdf(1) },
            new() { Name = "even.pdf", Pdf = Pdf(2) },
            new() { Name = "odd-again.pdf", Pdf = Pdf(1) },
        ];
        using var engine = L.Engine();
        Assert.Equal(4, PageCount(engine.Merge(sources)));
        Assert.Equal(5, PageCount(engine.Merge(sources, new MergeOptions { DoubleSided = true })));
    }

    [Fact]
    public void A_file_that_is_not_a_pdf_is_an_engine_error_that_names_it()
    {
        using var engine = L.Engine();
        var error = Assert.Throws<EngineException>(() => engine.Merge([
            new MergeSource { Name = "a.pdf", Pdf = Pdf() },
            new MergeSource { Name = "notes.txt", Pdf = Encoding.UTF8.GetBytes("hello") },
        ]));
        Assert.Equal("notes.txt: the file is not a PDF: it doesn't start with %PDF-", error.Message);
    }

    [Fact]
    public void A_file_whose_owner_restricts_printing_is_refused_unless_allow_restricted_is_set()
    {
        var limited = Pdf(1, "<output encrypt=\"aes-256\" allow=\"print\"/>", new RenderOptions { OwnerPassword = "owner" });
        using var engine = L.Engine();
        var error = Assert.Throws<EngineException>(() => engine.Merge([new MergeSource { Name = "limited.pdf", Pdf = limited }]));
        Assert.Equal(
            "limited.pdf: the owner restricts printing or copying: set the allow-restricted option to merge it anyway",
            error.Message);

        var output = engine.Merge([new MergeSource { Name = "limited.pdf", Pdf = limited }], new MergeOptions { AllowRestricted = true });
        Assert.Equal(1, PageCount(output));
        Assert.DoesNotContain("/Encrypt", Text(output));
    }

    [Fact]
    public void An_empty_list_is_an_engine_error()
    {
        using var engine = L.Engine();
        var error = Assert.Throws<EngineException>(() => engine.Merge([]));
        Assert.Equal("no PDFs were given: give merge at least one", error.Message);
    }

    [Fact]
    public void A_source_that_is_null_or_has_no_bytes_is_an_argument_error()
    {
        using var engine = L.Engine();
        Assert.Throws<ArgumentNullException>(() => engine.Merge(null!));
        Assert.Throws<ArgumentException>(() => engine.Merge([null!]));
        var error = Assert.Throws<ArgumentException>(() => engine.Merge([new MergeSource { Name = "a.pdf", Pdf = null! }]));
        Assert.StartsWith("source 1 (a.pdf): Pdf must be the PDF's bytes.", error.Message);
    }
}
