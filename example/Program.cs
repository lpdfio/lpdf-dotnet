/// Minimal end-to-end example: render lpdf's examples, encrypt a fixture, and fill an invoice with data,
/// using PdfEngine.
///
/// Run from lpdf's root (after 'make build-sdk-dotnet'):
///   make example-dotnet
///
/// Output: tmp/sdk-examples/ in lpdf's root.

using System.Text.Json;
using System.Text.RegularExpressions;
using Lpdf;
using Lpdf.Engine;

var lpdf     = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../../"));
var examples = Path.Combine(lpdf, "examples");
var fixtures = Path.Combine(lpdf, "test/fixtures");
var output   = Path.Combine(lpdf, "tmp/sdk-examples");
Directory.CreateDirectory(output);

// ── examples/ ────────────────────────────────────────────────────────────────
// Each folder examples/<n>-<id>/ has its document.xml, its document.json if it has data, and the fonts and
// images its <assets> name, by paths relative to the folder. A fresh engine for each example, so a font one
// of them names does not reach the next; its SrcFallback reads those paths from the example's folder.

var folders = Directory.GetDirectories(examples)
    .Where(folder => Regex.IsMatch(Path.GetFileName(folder), @"^\d+-"))
    .OrderBy(folder => int.Parse(Path.GetFileName(folder).Split('-')[0]));

foreach (var folder in folders)
{
    var name     = Path.GetFileName(folder);
    var xml      = await File.ReadAllTextAsync(Path.Combine(folder, "document.xml"));
    var dataFile = Path.Combine(folder, "document.json");

    var engine = L.Engine(new EngineOptions { SrcFallback = src => File.ReadAllBytes(Path.Combine(folder, src)) });
    var bytes  = File.Exists(dataFile)
        ? await engine.Render(xml, new RenderOptions { Data = JsonSerializer.Deserialize<object>(await File.ReadAllTextAsync(dataFile)) })
        : await engine.Render(xml);

    var outputFile = $"{name}-dotnet.pdf";
    await File.WriteAllBytesAsync(Path.Combine(output, outputFile), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}

// ── encrypt-permissions-only ─────────────────────────────────────────────────
// The fixture's <output> asks for AES-256 and allows neither copying nor modifying. No open password;
// cooperative viewers enforce its allow.
{
    var encXml = await File.ReadAllTextAsync(Path.Combine(fixtures, "showcase-encryption.xml"));
    const string outputFile = "encrypt-permissions-only-dotnet.pdf";

    var encEngine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });
    var bytes = await encEngine.Render(encXml, new RenderOptions { OwnerPassword = "s3cr3t" });
    await File.WriteAllBytesAsync(Path.Combine(output, outputFile), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}

// ── encrypt-open-password ────────────────────────────────────────────────────
// Viewers prompt for "password" before displaying content. The cipher and the permissions come from the
// fixture's <output>, the passwords from the render.
{
    var encXml = await File.ReadAllTextAsync(Path.Combine(fixtures, "showcase-encryption.xml"));
    const string outputFile = "encrypt-open-password-dotnet.pdf";

    var encEngine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });
    var bytes = await encEngine.Render(encXml, new RenderOptions { UserPassword = "password", OwnerPassword = "owner" });
    await File.WriteAllBytesAsync(Path.Combine(output, outputFile), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}

// ── example-data ─────────────────────────────────────────────────────────────
// Render the invoice in examples/ with its data from document.json.
{
    var xml      = await File.ReadAllTextAsync(Path.Combine(examples, "2-invoice/document.xml"));
    var dataJson = await File.ReadAllTextAsync(Path.Combine(examples, "2-invoice/document.json"));
    var data     = JsonSerializer.Deserialize<object>(dataJson);
    const string outputFile = "example-data-dotnet.pdf";

    var dataEngine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });
    var bytes = await dataEngine.Render(xml, new RenderOptions { Data = data });
    await File.WriteAllBytesAsync(Path.Combine(output, outputFile), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}
