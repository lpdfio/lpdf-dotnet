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
