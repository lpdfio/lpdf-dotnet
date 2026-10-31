using System.Text;
using System.Text.RegularExpressions;
using Lpdf;
using Lpdf.Engine;
using Xunit;

namespace Lpdf.Tests;

/// <summary>
/// The <c>src</c> of a font, an image, a file and a file of Markdown rules is found in an XML document as the engine
/// reads the document: entities decoded, a <c>&gt;</c> in a quoted value, either quote, and what is in a comment or a
/// CDATA section is text.
/// </summary>
public class XmlSrcTests
{
    private static readonly byte[] Pixel = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg==");

    private const string RedTitles = "<markdown-styles><h1 color=\"#ff0000\"/></markdown-styles>";

    /// <summary>A <c>SrcFallback</c> that hands out the files it holds and keeps each <c>src</c> it is asked for.</summary>
    private sealed class RecordingFiles(Dictionary<string, byte[]> files)
    {
        public List<string> Asked { get; } = [];

        public byte[] Read(string src)
        {
            Asked.Add(src);
            return files.TryGetValue(src, out var bytes) ? bytes : throw new FileNotFoundException("no such file", src);
        }
    }

    private static string Xml(string assets = "", string attachments = "", string section = "<layout><text>x</text></layout>") =>
        $"<lpdf version=\"1\"><assets>{assets}</assets><document><attachments>{attachments}</attachments>" +
        $"<section>{section}</section></document></lpdf>";

    private static string MarkdownXml(string styles) =>
        Xml(section: $"<markdown>{styles}<markdown-content># Title</markdown-content></markdown>");

    private static PdfEngine Engine(RecordingFiles files) =>
        L.Engine(new EngineOptions { SrcFallback = files.Read }).SetLicenseKey("test-key");

    private static string Normalised(byte[] pdf)
    {
        var text = Encoding.Latin1.GetString(pdf);
        text = Regex.Replace(text, @"/CreationDate[^\n]*", "");
        return Regex.Replace(text, @"/ID *\[[^\]]*\]", "");
    }

    [Theory]
    [InlineData("<font name=\"brand\" ref=\"R&amp;D Sans\" src=\"R&amp;D Sans.ttf\"/>", "R&D Sans.ttf", "R&D Sans")]
    [InlineData("<font name=\"brand\" ref=\"rows > 1000\" src=\"brand.ttf\"/>", "brand.ttf", "rows > 1000")]
    [InlineData("<font name='brand' src='brand.ttf'/>", "brand.ttf", "brand")]
    public async Task A_font_is_read_from_its_src_and_loaded_under_its_ref_or_name(string font, string src, string key)
    {
        var files = new RecordingFiles(new() { [src] = "not a font"u8.ToArray() });
        using var engine = Engine(files);

        // The bytes are not a font, so the engine says which font it was given them for.
        var error = await Assert.ThrowsAsync<EngineException>(
            () => engine.Render(Xml(assets: font, section: "<layout><text font=\"brand\">x</text></layout>")));

        Assert.Equal(new[] { src }, files.Asked);
        Assert.Contains($"font '{key}'", error.Message);
    }

    [Theory]
    [InlineData("<image name=\"logo\" ref=\"R&amp;D logo\" src=\"R&amp;D logo.png\"/>", "R&D logo.png")]
    [InlineData("<image name=\"logo\" ref=\"rows > 1000\" src=\"logo.png\"/>", "logo.png")]
    [InlineData("<image name='logo' src='logo.png'/>", "logo.png")]
    public async Task An_image_is_read_from_its_src_and_loaded_under_its_ref_or_name(string image, string src)
    {
        var files = new RecordingFiles(new() { [src] = Pixel });
        using var engine = Engine(files);

        // The engine refuses an image it has no bytes for under its ref, or with no ref its name.
        var pdf = await engine.Render(Xml(assets: image, section: "<layout><img name=\"logo\" width=\"40pt\"/></layout>"));

        Assert.Equal(new[] { src }, files.Asked);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdf, 0, 5));
    }

    [Theory]
    [InlineData("<file name=\"R&amp;D.csv\" src=\"R&amp;D.csv\"/>", "R&D.csv")]
    [InlineData("<file name=\"rows.csv\" description=\"rows > 1000\" src=\"rows.csv\"/>", "rows.csv")]
    [InlineData("<file name='rows.csv' src='rows.csv'/>", "rows.csv")]
    public async Task A_file_is_read_from_its_src_and_loaded_under_its_ref_or_name(string file, string src)
    {
        var files = new RecordingFiles(new() { [src] = "a,b"u8.ToArray() });
        using var engine = Engine(files);

        // The engine refuses a file it has no bytes for under its ref, or with no ref its name.
        var pdf = await engine.Render(Xml(attachments: file));

        Assert.Equal(new[] { src }, files.Asked);
        Assert.Single(Regex.Matches(Encoding.Latin1.GetString(pdf), "/Type /EmbeddedFile"));
    }

    [Theory]
    [InlineData("<markdown-styles src=\"R&amp;D styles.xml\"/>", "R&D styles.xml")]
    [InlineData("<markdown-styles xmlns:rows=\"urn:rows>1000\" src=\"styles.xml\"/>", "styles.xml")]
    [InlineData("<markdown-styles src='styles.xml'/>", "styles.xml")]
    public async Task A_file_of_rules_is_read_from_the_src_of_its_styles(string styles, string src)
    {
        var files = new RecordingFiles(new() { [src] = Encoding.UTF8.GetBytes(RedTitles) });
        using var engine = Engine(files);
        var inline = await engine.Render(MarkdownXml(RedTitles));

        // The engine refuses a src whose file was not loaded.
        var fromSrc = await engine.Render(MarkdownXml(styles));

        Assert.Equal(new[] { src }, files.Asked);
        Assert.Equal(Normalised(inline), Normalised(fromSrc));
    }

    [Theory]
    [InlineData("<assets><font name=\"brand\" src=\"hidden.ttf\"/></assets>")]
    [InlineData("<assets><image name=\"logo\" src=\"hidden.png\"/></assets>")]
    [InlineData("<attachments><file name=\"rows.csv\" src=\"hidden.csv\"/></attachments>")]
    [InlineData("<markdown><markdown-styles src=\"hidden.xml\"/></markdown>")]
    public async Task A_src_in_a_comment_or_a_CDATA_section_is_text_and_is_not_read(string declaration)
    {
        var files = new RecordingFiles(new());
        using var engine = Engine(files);

        var pdf = await engine.Render(Xml(section: $"<layout><!-- {declaration} --><code><![CDATA[{declaration}]]></code></layout>"));

        Assert.Empty(files.Asked);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(pdf, 0, 5));
    }
}
