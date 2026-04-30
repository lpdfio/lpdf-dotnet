/// Minimal end-to-end example: render examples from the project root using PdfEngine.
///
/// Run (after 'make wasi' and 'dotnet build'):
///   dotnet run --project src/adapters/dotnet/example/LpdfExample.csproj

using Lpdf;
using Lpdf.Engine;

var root    = Path.Combine(AppContext.BaseDirectory, "../../../../../../../example/");
var fixtures = Path.Combine(AppContext.BaseDirectory, "../../../../../../../test/fixtures/");

// ── example1 / example2 ─────────────────────────────────────────────────────

var examples = new[] { "example1", "example2" };

var engine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });

engine.LoadFont("montserrat", await File.ReadAllBytesAsync(Path.Combine(root, "assets/fonts/Montserrat-Regular.ttf")));
engine.LoadImage("logo", await File.ReadAllBytesAsync(Path.Combine(AppContext.BaseDirectory, "../../../../lpdf-light.png")));

foreach (var example in examples)
{
    var xml   = await File.ReadAllTextAsync(Path.Combine(root, $"xml/{example}.xml"));
    var bytes = await engine.Render(xml);
    var outputFile = $"{example}-dotnet.pdf";
    await File.WriteAllBytesAsync(Path.Combine(root, $"result/{outputFile}"), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}

// ── encrypt-permissions-only ─────────────────────────────────────────────────
// No open password; cooperative viewers enforce Print = false, Copy = false.
{
    var encXml = await File.ReadAllTextAsync(Path.Combine(fixtures, "showcase-encryption.xml"));
    const string outputFile = "encrypt-permissions-only-dotnet.pdf";

    var encEngine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });
    encEngine.SetEncryption(new EncryptOptions
    {
        UserPassword  = "",
        OwnerPassword = "s3cr3t",
        Permissions   = new EncryptPermissions { Print = false, Copy = false },
    });

    var bytes = await encEngine.Render(encXml);
    await File.WriteAllBytesAsync(Path.Combine(root, $"result/{outputFile}"), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}

// ── encrypt-open-password ────────────────────────────────────────────────────
// Viewers prompt for "password" before displaying content.
{
    var encXml = await File.ReadAllTextAsync(Path.Combine(fixtures, "showcase-encryption.xml"));
    const string outputFile = "encrypt-open-password-dotnet.pdf";

    var encEngine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });
    encEngine.SetEncryption(new EncryptOptions
    {
        UserPassword  = "password",
        OwnerPassword = "owner",
        Permissions   = new EncryptPermissions { Copy = false },
    });

    var bytes = await encEngine.Render(encXml);
    await File.WriteAllBytesAsync(Path.Combine(root, $"result/{outputFile}"), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}

// ── example-data ─────────────────────────────────────────────────────────────
// Render data-invoice.xml with dynamic data from data-invoice.json.
{
    var xml      = await File.ReadAllTextAsync(Path.Combine(root, "xml/data-invoice.xml"));
    var dataJson = await File.ReadAllTextAsync(Path.Combine(root, "xml/data-invoice.json"));
    var data     = System.Text.Json.JsonSerializer.Deserialize<object>(dataJson);
    const string outputFile = "example-data-dotnet.pdf";

    var dataEngine = L.Engine(new EngineOptions { SrcFallback = File.ReadAllBytes });
    var bytes = await dataEngine.Render(xml, new RenderOptions { Data = data });
    await File.WriteAllBytesAsync(Path.Combine(root, $"result/{outputFile}"), bytes);
    Console.WriteLine($"output: {outputFile} ({bytes.Length:N0} bytes)");
}
