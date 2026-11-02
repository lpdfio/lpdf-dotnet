using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Lpdf.Engine;
using Xunit;

namespace Lpdf.Tests;

/// <summary>Stamps: PNG images drawn over the pages of an existing PDF, through the engine's <c>stamp</c> request.</summary>
public class StampTests
{
    /// <summary>A PNG chunk; the decoder doesn't check the CRC, which is left 0.</summary>
    private static byte[] Chunk(string tag, byte[] data)
    {
        var length = BitConverter.GetBytes(data.Length);
        if (BitConverter.IsLittleEndian) Array.Reverse(length);
        return [.. length, .. Encoding.ASCII.GetBytes(tag), .. data, 0, 0, 0, 0];
    }

    /// <summary>An RGB PNG of <paramref name="width"/> by <paramref name="height"/> pixels, all red.</summary>
    private static byte[] Png(int width, int height)
    {
        byte[] header = [0, 0, (byte)(width >> 8), (byte)width, 0, 0, (byte)(height >> 8), (byte)height, 8, 2, 0, 0, 0];
        var raw = new List<byte>();
        for (var y = 0; y < height; y++)
        {
            raw.Add(0);
            for (var x = 0; x < width; x++) raw.AddRange([200, 40, 40]);
        }
        using var packed = new MemoryStream();
        using (var zlib = new ZLibStream(packed, CompressionLevel.Optimal)) zlib.Write(raw.ToArray());
        return [
            .. Convert.FromHexString("89504e470d0a1a0a"), .. Chunk("IHDR", header), .. Chunk("IDAT", packed.ToArray()),
            .. Chunk("IEND", []),
        ];
    }

    /// <summary>A PDF of <paramref name="pages"/> pages, rendered by the engine.</summary>
    private static byte[] Pdf(int pages = 2)
    {
        var sections = string.Concat(Enumerable.Range(1, pages).Select(i => $"<section><layout><text>Page {i}</text></layout></section>"));
        using var engine = L.Engine();
        return engine.Render($"<lpdf version=\"1\"><document>{sections}</document></lpdf>");
    }

    /// <summary>The text of what a stamp appended to <paramref name="input"/>.</summary>
    private static string Update(byte[] input, byte[] output) => Encoding.Latin1.GetString(output, input.Length, output.Length - input.Length);

    [Fact]
    public void A_stamp_returns_the_input_with_an_update_after_it_that_draws_the_image()
    {
        var input = Pdf();
        using var engine = L.Engine();
        var output = engine.Stamp(input, [
            new StampAttr { Image = Png(60, 30), Page = "last", X = "72pt", Y = "72pt", Width = "144pt", Height = "36pt", Justify = "end" },
        ]);
        Assert.True(output.Length > input.Length);
        Assert.Equal(input, output[..input.Length]);
        Assert.Contains("/LpdfStamp0 Do", Update(input, output));
    }

    [Fact]
    public void An_image_given_to_several_stamps_is_embedded_once()
    {
        var input = Pdf(3);
        var seal = Png(40, 40);
        using var engine = L.Engine();
        var output = engine.Stamp(input, [
            new StampAttr { Image = seal, Page = "each", Anchor = "top-right", X = "-36pt", Y = "36pt", Width = "40pt", Height = "40pt" },
            new StampAttr { Image = seal, Page = "1", Anchor = "center", Width = "40pt", Height = "40pt", MaxGrow = 2 },
            new StampAttr { Image = Png(20, 10), Page = "2-3", X = "1in", Y = "1in", Width = "1in", Height = "1in" },
        ]);
        Assert.Equal(2, Regex.Matches(Update(input, output), @"/Subtype\s*/Image").Count);
    }

    [Fact]
    public void Without_a_license_key_each_stamp_gets_the_attribution_line_linked_to_lpdf_io()
    {
        var input = Pdf(1);
        using var engine = L.Engine();
        var output = engine.Stamp(input, [new StampAttr { Image = Png(10, 10), Page = "1", Anchor = "center", Width = "50pt", Height = "50pt" }]);
        Assert.Contains("(https://lpdf.io)", Update(input, output));
    }

    [Fact]
    public void A_page_the_pdf_does_not_have_and_a_box_with_no_position_are_engine_errors()
    {
        var input = Pdf(2);
        using var engine = L.Engine();
        var past = Assert.Throws<EngineException>(() => engine.Stamp(input, [
            new StampAttr { Image = Png(10, 10), Page = "3", X = "0pt", Y = "0pt", Width = "10pt", Height = "10pt" },
        ]));
        Assert.Equal("stamp 1: the PDF has no page 3: it has 2 pages", past.Message);
        var nowhere = Assert.Throws<EngineException>(() => engine.Stamp(input, [
            new StampAttr { Image = Png(10, 10), Page = "1", Width = "10pt", Height = "10pt" },
        ]));
        Assert.Equal("stamp 1: has no position: give it x and y, or an anchor", nowhere.Message);
        var jpeg = Assert.Throws<EngineException>(() => engine.Stamp(input, [
            new StampAttr { Image = [0xFF, 0xD8, 0xFF, 0xE0, 0, 0], Page = "1", X = "0pt", Y = "0pt", Width = "10pt", Height = "10pt" },
        ]));
        Assert.Contains("the image is a JPEG, and a stamp takes a PNG", jpeg.Message);
    }
}
