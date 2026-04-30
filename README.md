<p align="center"><img src="lpdf-light.png" height="48" alt="Lpdf"></p>

# Lpdfio.Lpdf

**.NET SDK for [Lpdf](https://lpdf.io) — PDF as Code on every platform**

You describe a document as code or XML. Lpdf renders a compact, pixel-perfect PDF — identical across platforms.

## Installation

```bash
dotnet add package Lpdfio.Lpdf
```

## Usage

```csharp
using Lpdf;

var engine = L.Engine();

var doc = L.Document(new() { Size = "letter", Margin = "48pt" }, [
    L.Section(NoAttr, [
        L.Layout(NoAttr, [
            L.Stack(new() { Gap = "24pt" }, [
                L.Split(NoAttr, [
                    L.Text(new() { FontSize = "8pt", Color = "#888888" }, ["ACME CORP"]),
                    L.Text(new() { FontSize = "22pt", Bold = "true" }, ["Project Proposal"]),
                ]),
                L.Divider(new() { Thickness = "xs" }),
                L.Text(new() { FontSize = "13pt", Bold = "true" }, ["Scope of Work"]),
                L.Flank(new() { Gap = "12pt", Align = "start" }, [
                    L.Text(new() { Color = "#888888", Width = "24pt" }, ["01"]),
                    L.Text(NoAttr, ["Discovery & Research"]),
                ]),
            ]),
        ]),
    ]),
]);

var pdf = await engine.Render(doc);
```

## Requirements

- .NET 8+
- No external runtime dependencies — the Wasmtime runtime and WASI binary are bundled in the package.

## Docs

[lpdf.io/docs/dotnet](https://lpdf.io/docs/dotnet)

--

Dual-licensed: Community License (free) and Commercial License (paid). See [LICENSE](LICENSE) for full terms.
