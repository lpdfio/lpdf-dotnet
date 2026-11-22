![Lpdf - PDF as Code](https://raw.githubusercontent.com/lpdfio/lpdf-dotnet/main/lpdf-mark.svg)

# Lpdfio.Lpdf

`Lpdfio.Lpdf` is the [Lpdf](https://lpdf.io?utm_campaign=sdk-dotnet&utm_medium=referral&utm_source=readme) C# SDK: render PDF documents from XML or from code.

## Installation

```bash
dotnet add package Lpdfio.Lpdf
```

## Usage

```csharp
using Lpdf;
using static Lpdf.L;

var engine = L.Engine();

var doc = L.Document(new() { Size = "letter" }, [
    L.Section(NoAttr, [
        L.Layout(new() { Margin = "48pt" }, [
            L.Column(new() { Gap = "24pt" }, [
                L.Row(new() { Justify = "between" }, [
                    L.Text(new() { FontSize = "8pt", Color = "#888888" }, ["ACME CORP"]),
                    L.Text(new() { FontSize = "22pt", Bold = true }, ["Project Proposal"]),
                ]),
                L.Divider(new() { Thickness = "xs" }),
                L.Text(new() { FontSize = "13pt", Bold = true }, ["Scope of Work"]),
                L.Row(new() { Gap = "12pt", Align = "start" }, [
                    L.Text(new() { Color = "#888888", Width = "24pt" }, ["01"]),
                    L.Text(new() { Width = "fill" }, ["Discovery & Research"]),
                ]),
            ]),
        ]),
    ]),
]);

var pdf = engine.Render(doc);
```

## Requirements

- .NET 8 or later
- No external runtime dependencies: the Wasmtime runtime and the WASI binary are bundled in the package

## Docs

[lpdf.io/docs](https://lpdf.io/docs/install/?sdk=dotnet&utm_campaign=sdk-dotnet&utm_medium=referral&utm_source=readme)

## Issues

Report bugs and request features at [github.com/lpdfio/lpdf/issues](https://github.com/lpdfio/lpdf/issues), the one tracker for the engine, the SDKs and the VS Code extension. Pull requests are not accepted.

---

Licensed under the Lpdf License. See [LICENSE](https://github.com/lpdfio/lpdf-dotnet/blob/main/LICENSE) for full terms.
