<p align="center"><img src="lpdf-light.png" height="48" alt="Lpdf"></p>

# Lpdfio.Lpdf

.NET adapter for [Lpdf](https://lpdf.io) — an accurate, efficient, and cross-platform PDF engine.

## Installation

```bash
dotnet add package Lpdfio.Lpdf
```

## Usage

```csharp
using Lpdf;
using Lpdf.Engine;

var engine = new LpdfEngine(
    licenseKey: "",
    options: new EngineOptions { SrcFallback = File.ReadAllBytes });

engine.LoadFont("montserrat", await File.ReadAllBytesAsync("fonts/Montserrat-Regular.ttf"));
engine.LoadImage("logo", await File.ReadAllBytesAsync("images/logo.png"));

var xml   = await File.ReadAllTextAsync("document.xml");
var bytes = await engine.RenderPdf(xml);

await File.WriteAllBytesAsync("output.pdf", bytes);
```

## XML format

Documents are defined in a layout XML format. See the [Lpdf documentation](https://lpdf.io/docs) and [examples](https://github.com/lpdfio/lpdf/tree/main/docs/examples) for the full schema.

```xml
<stack spacing="m" padding="l">
  <text font-size="xl" font="Montserrat-Bold">Invoice #1001</text>
  <grid columns="2">
    <text>Date</text>      <text>2026-04-25</text>
    <text>Due</text>       <text>2026-05-25</text>
  </grid>
</stack>
```

## License

Dual-licensed: Community License (free) and Commercial License (paid). See [LICENSE](LICENSE) for full terms.

Third-party component licenses are listed in `THIRD_PARTY_LICENSES`.
