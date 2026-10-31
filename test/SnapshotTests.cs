using Lpdf;
using Lpdf.Engine;
using Lpdf.Kit;
using Xunit;

namespace Lpdf.Tests;

/// <summary>
/// Snapshot tests: render each fixture XML → PDF, SHA-256 the bytes, compare
/// against the stored golden value in <c>test/snapshots/</c>.
///
/// The snapshot files are shared with the Node adapter — byte-identical output
/// from the same Rust core means the hashes should match across adapters.
///
/// To regenerate hashes after an intentional rendering change:
/// <code>
///   UPDATE_SNAPSHOTS=1 dotnet test src/adapters/dotnet
/// </code>
/// </summary>
public class SnapshotTests
{
    [Theory]
    [InlineData("example1")]
    [InlineData("example2")]
    [InlineData("example3")]
    [InlineData("example4")]
    [InlineData("example5")]
    [InlineData("example6")]
    [InlineData("example7")]
    [InlineData("example8")]
    [InlineData("example9")]
    [InlineData("example10")]
    [InlineData("example11")]
    [InlineData("showcase-row")]
    [InlineData("showcase-row-fill")]
    [InlineData("showcase-frame")]
    [InlineData("showcase-grid")]
    [InlineData("showcase-row-between")]
    [InlineData("showcase-column")]
    [InlineData("showcase-table")]
    [InlineData("showcase-barcode")]
    [InlineData("showcase-encryption")]
    [InlineData("showcase-forms")]
    [InlineData("bench_xs")]
    [InlineData("bench_s")]
    [InlineData("bench_m")]
    [InlineData("bench_l")]
    [InlineData("bench_xl")]
    public async Task FixtureMatchesStoredHash(string name)
    {
        if (!SnapshotHelper.HAS_FIXTURES)
            return; // Fixture files not available outside the monorepo.

        var xml   = File.ReadAllText(Path.Combine(SnapshotHelper.Fixtures, $"{name}.xml"));
        using var engine = L.Engine().SetLicenseKey("test-key");
        // showcase-encryption asks for encryption in its output: its snapshot is the file unencrypted.
        var bytes = await engine.Render(xml, new RenderOptions { SkipEncryption = true });
        SnapshotHelper.CompareOrUpdate(name, bytes);
    }
}

/// <summary>
/// Engine feature tests: encryption and image loading.
/// </summary>
public class EngineFeatureTests
{
    private static readonly string _minimalXml =
        "<lpdf version=\"1\"><document><section><layout></layout></section></document></lpdf>";

    /// <summary>A document whose output has <paramref name="attributes"/>: by default, one that asks for AES-256.</summary>
    private static string EncryptedXml(string attributes = "encrypt=\"aes-256\"")
        => $"<lpdf version=\"1\"><document><output {attributes}/><section><layout><text>Hello</text></layout></section></document></lpdf>";

    private static readonly RenderOptions _ownerPassword = new() { OwnerPassword = "s3cr3t" };

    [Fact]
    public async Task Encrypts_a_document_whose_output_asks_for_it_with_the_passwords_of_the_render()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var bytes = await engine.Render(EncryptedXml(), new RenderOptions { UserPassword = "open", OwnerPassword = "s3cr3t" });

        Assert.True(
            bytes[..5].SequenceEqual(System.Text.Encoding.ASCII.GetBytes("%PDF-")),
            "Output must start with %PDF-");
        var text = System.Text.Encoding.Latin1.GetString(bytes);
        Assert.Contains("/Encrypt", text);
        Assert.Contains("/V 5", text);
        Assert.Contains("/R 6", text);
        Assert.Contains("/CFM /AESV3", text);
    }

    [Fact]
    public async Task Takes_the_cipher_from_the_output()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var text = System.Text.Encoding.Latin1.GetString(await engine.Render(EncryptedXml("encrypt=\"rc4-128\""), _ownerPassword));

        Assert.Contains("/V 2", text);
        Assert.Contains("/R 3", text);
        Assert.DoesNotContain("AESV3", text);
    }

    [Fact]
    public async Task Takes_the_permissions_from_allow()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var text = System.Text.Encoding.Latin1.GetString(await engine.Render(EncryptedXml("encrypt=\"rc4-128\" allow=\"print\""), _ownerPassword));

        // Printing, at full quality too, and nothing else.
        Assert.Contains("/P -1852", text);
    }

    [Fact]
    public async Task Every_aes256_render_gets_a_fresh_key()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");

        var first  = await engine.Render(EncryptedXml(), _ownerPassword);
        var second = await engine.Render(EncryptedXml(), _ownerPassword);

        Assert.False(first.SequenceEqual(second), "Two AES-256 renders must differ.");
    }

    [Fact]
    public async Task Rc4_renders_of_one_document_are_identical()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");

        var first  = await engine.Render(EncryptedXml("encrypt=\"rc4-128\""), _ownerPassword);
        var second = await engine.Render(EncryptedXml("encrypt=\"rc4-128\""), _ownerPassword);

        Assert.True(first.SequenceEqual(second));
    }

    [Fact]
    public async Task Skip_encryption_renders_a_document_that_asks_for_encryption_unencrypted()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var text = System.Text.Encoding.Latin1.GetString(await engine.Render(EncryptedXml(), new RenderOptions { SkipEncryption = true }));

        Assert.DoesNotContain("/Encrypt", text);
    }

    public static TheoryData<string, string, string?, bool, string> RefusedRenders => new()
    {
        { "a document that asks for encryption without the owner password", EncryptedXml(), null, false, "no owner password" },
        { "passwords for a document that does not ask for encryption", _minimalXml, "s3cr3t", false, "does not ask for encryption" },
        { "passwords with SkipEncryption", EncryptedXml(), "s3cr3t", true, "not both" },
    };

    [Theory]
    [MemberData(nameof(RefusedRenders))]
    public async Task Refuses_a_render_and_says_what_to_change(string what, string xml, string? ownerPassword, bool skip, string expected)
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var error = await Assert.ThrowsAsync<EngineException>(
            () => engine.Render(xml, new RenderOptions { OwnerPassword = ownerPassword, SkipEncryption = skip }));

        Assert.True(error.Message.Contains(expected), $"{what}: {error.Message}");
    }

    /// <summary>
    /// A document with no text of its own that asks for PDF/A: the attribution line's name, in its embedded face, is
    /// its one text.
    /// </summary>
    private static string PdfAXml(string level = "2b", string body = "<column background=\"#e8f0fc\" height=\"40pt\"/>")
        => $"<lpdf version=\"1\"><document><output pdfa=\"{level}\"/><section><layout>{body}</layout></section></document></lpdf>";

    [Fact]
    public async Task Writes_pdfa_when_the_output_asks_for_it_dated_by_created_on()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var text = System.Text.Encoding.Latin1.GetString(
            await engine.Render(PdfAXml("2u"), new RenderOptions { CreatedOn = "2026-10-31T09:30:00Z" }));

        Assert.Contains("<pdfaid:part>2</pdfaid:part>", text);
        Assert.Contains("<pdfaid:conformance>U</pdfaid:conformance>", text);
        Assert.Contains("/S /GTS_PDFA1", text);
        Assert.Contains("/ModDate (D:20261031093000Z)", text);
    }

    [Fact]
    public async Task Takes_pdfa_from_the_output_of_a_kit_document()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var document = L.Document(
            new DocumentAttr(Output: new DocumentOutput(PdfA: "2b")),
            [L.Section(null, [L.Layout(null, [L.Column(new Lpdf.Layout.ColumnAttr { Background = "#e8f0fc", Height = "40pt" })])])]);
        var text = System.Text.Encoding.Latin1.GetString(
            await engine.Render(document, new RenderOptions { CreatedOn = "2026-10-31" }));

        Assert.Contains("<pdfaid:conformance>B</pdfaid:conformance>", text);
    }

    [Fact]
    public async Task Refuses_pdfa_without_created_on_and_text_in_a_built_in_font()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var missing = await Assert.ThrowsAsync<EngineException>(() => engine.Render(PdfAXml()));
        Assert.Contains("created_on", missing.Message);

        var builtIn = await Assert.ThrowsAsync<EngineException>(
            () => engine.Render(PdfAXml("2b", "<text>Hello</text>"), new RenderOptions { CreatedOn = "2026-10-31" }));
        Assert.Contains("\"Hello\" is in Helvetica", builtIn.Message);
    }

    [Fact]
    public async Task Encrypts_a_document_built_with_the_kit_as_its_output_asks()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var document = L.Document(
            new DocumentAttr(Output: new DocumentOutput(Encrypt: Cipher.Aes256, Allow: $"{Permission.Print} {Permission.FillForms}")),
            [L.Section(null, [L.Layout(null, [L.Text(null, ["Hello PDF"])])])]);
        var text = System.Text.Encoding.Latin1.GetString(await engine.Render(document, _ownerPassword));

        Assert.Contains("/CFM /AESV3", text);
        await Assert.ThrowsAsync<EngineException>(() => engine.Render(document));
    }

    private static PdfDocument WithAttachments(params FileAttr[] files)
        => L.Document(new DocumentAttr(Attachments: files), [L.Section(null, [L.Layout(null, [L.Text(null, ["Invoice"])])])]);

    private static int EmbeddedFiles(byte[] pdf)
        => System.Text.RegularExpressions.Regex.Matches(System.Text.Encoding.Latin1.GetString(pdf), "/Type /EmbeddedFile").Count;

    [Fact]
    public async Task Embeds_the_files_a_document_carries_from_LoadFile_or_their_src()
    {
        var terms = Path.GetTempFileName();
        File.WriteAllText(terms, "Terms of sale");
        try
        {
            var document = WithAttachments(
                new FileAttr { Name = "invoice.xml", Ref = "invoice", Mime = "text/xml", Relationship = FileRelationship.Data },
                new FileAttr { Name = "terms.txt", Src = terms, Description = "The terms" });
            using var engine = L.Engine(new() { SrcFallback = File.ReadAllBytes }).SetLicenseKey("test-key");
            engine.LoadFile("invoice", System.Text.Encoding.UTF8.GetBytes("<invoice/>"));

            var pdf = await engine.Render(document);
            Assert.Equal(2, EmbeddedFiles(pdf));
            Assert.Contains("/AFRelationship /Data", System.Text.Encoding.Latin1.GetString(pdf));
            Assert.Contains("/Subtype /text#2Fxml", System.Text.Encoding.Latin1.GetString(pdf));
            Assert.Equal(pdf, await engine.Render(await L.ToXml(document)));
        }
        finally
        {
            File.Delete(terms);
        }
    }

    [Fact]
    public async Task A_file_src_without_a_SrcFallback_or_that_cannot_be_read_is_an_error_that_names_it()
    {
        var document = WithAttachments(new FileAttr { Name = "data.csv", Src = "/no/such/data.csv" });

        using var plain = L.Engine().SetLicenseKey("test-key");
        var noFallback = await Assert.ThrowsAsync<EngineException>(() => plain.Render(document));
        Assert.Contains("SrcFallback", noFallback.Message);
        Assert.Contains("data.csv", noFallback.Message);

        using var reading = L.Engine(new() { SrcFallback = File.ReadAllBytes }).SetLicenseKey("test-key");
        var unreadable = await Assert.ThrowsAsync<EngineException>(() => reading.Render(document));
        Assert.Contains("cannot read its src '/no/such/data.csv'", unreadable.Message);
    }

    [Fact]
    public async Task Reads_the_src_of_a_file_as_the_engine_reads_its_XML()
    {
        var folder = Directory.CreateTempSubdirectory("lpdf-files-");
        try
        {
            File.WriteAllText(Path.Combine(folder.FullName, "R&D terms.txt"), "Terms");
            var document = WithAttachments(
                new FileAttr { Name = "Terms & Conditions.txt", Src = Path.Combine(folder.FullName, "R&D terms.txt"), Description = "Rows > 100" });
            var xml = (await L.ToXml(document))
                .Replace("<attachments>", "<attachments><!-- <file name=\"old.csv\" src=\"/no/such/old.csv\"/> -->");

            using var engine = L.Engine(new() { SrcFallback = File.ReadAllBytes }).SetLicenseKey("test-key");
            Assert.Equal(1, EmbeddedFiles(await engine.Render(xml)));
            Assert.Equal(await engine.Render(document), await engine.Render(xml));
        }
        finally
        {
            folder.Delete(recursive: true);
        }
    }

    [Fact]
    public async Task A_file_with_no_bytes_is_an_error_that_says_how_to_load_it()
    {
        using var engine = L.Engine().SetLicenseKey("test-key");
        var error = await Assert.ThrowsAsync<EngineException>(() => engine.Render(WithAttachments(new FileAttr { Name = "a.xml" })));
        Assert.Contains("loadFile('a.xml', bytes)", error.Message);
    }

    [Fact]
    public async Task LoadImage_DoesNotThrowAndProducesValidPdf()
    {
        // A minimal 1×1 white grayscale PNG
        var png1x1 = Convert.FromHexString(
            "89504e470d0a1a0a0000000d49484452000000010000000108000000003a7e9b55" +
            "0000000a49444154789c6260000000020001e221bc330000000049454e44ae426082");

        using var engine = L.Engine().SetLicenseKey("test-key");
        engine.LoadImage("testimg", png1x1);
        var bytes = await engine.Render(_minimalXml);

        Assert.True(
            bytes[..5].SequenceEqual(System.Text.Encoding.ASCII.GetBytes("%PDF-")),
            "Output must start with %PDF-");
    }
}
