using System.Text.Json;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Shared;

namespace Lpdf;

/// <summary>
/// Stateful lpdf renderer.
/// Create via <see cref="Pdf.Engine"/>; call <see cref="Render(string, RenderOptions?)"/>
/// or <see cref="Render(PdfDocument, RenderOptions?)"/> as many times as needed.
/// </summary>
public sealed class PdfEngine : IDisposable
{
    private          string         _licenseKey = string.Empty;
    private readonly EngineOptions  _engineOpts;
    private readonly WasmRunner     _wasm;
    private readonly Dictionary<string, byte[]> _fonts  = new();
    private readonly Dictionary<string, byte[]> _images = new();
    private          bool           _disposed;
    private          EncryptOptions? _encrypt;

    /// <param name="options">Engine-level options (e.g. <c>SrcFallback</c>).</param>
    public PdfEngine(EngineOptions? options = null)
    {
        _engineOpts = options ?? new EngineOptions();
        _wasm       = new WasmRunner();
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Set the license key and return <c>this</c> for fluent chaining.
    /// Pass an empty string to render in evaluation mode (produces a visible watermark).
    /// </summary>
    public PdfEngine SetLicenseKey(string key)
    {
        ThrowIfDisposed();
        _licenseKey = key ?? throw new ArgumentNullException(nameof(key));
        return this;
    }

    /// <summary>
    /// Configure RC4-128 encryption for all subsequent <see cref="Render(string, RenderOptions?)"/> calls.
    /// Pass <see langword="null"/> to clear previously set encryption.
    /// </summary>
    public PdfEngine SetEncryption(EncryptOptions? options)
    {
        ThrowIfDisposed();
        _encrypt = options;
        return this;
    }

    /// <summary>Clear any previously set encryption. Equivalent to <c>SetEncryption(null)</c>.</summary>
    public PdfEngine ClearEncryption()
        => SetEncryption(null);

    /// <summary>
    /// Register raw TTF/OTF bytes for a font name referenced via <c>fonts src="…"</c>.
    /// Call before <see cref="Render(string, RenderOptions?)"/>. Returns <c>this</c>.
    /// </summary>
    public PdfEngine LoadFont(string name, byte[] bytes)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(bytes);
        _fonts[name] = bytes;
        return this;
    }

    /// <summary>
    /// Register raw PNG or JPEG bytes for an image name referenced via <c>&lt;img name="…"&gt;</c>.
    /// Call before <see cref="Render(string, RenderOptions?)"/>. Returns <c>this</c>.
    /// </summary>
    public PdfEngine LoadImage(string name, byte[] bytes)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(bytes);
        _images[name] = bytes;
        return this;
    }

    /// <summary>Render an lpdf XML string to PDF bytes.</summary>
    /// <param name="xml">Full lpdf XML document string.</param>
    /// <param name="callOptions">Per-call options (data, timestamp).</param>
    public Task<byte[]> Render(string xml, RenderOptions? callOptions = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(xml);
        var fonts  = _fonts.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_fonts : null;
        var images = _images.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_images : null;
        var encryptJson = BuildEncryptJson();
        var dataJson    = BuildDataJson(callOptions?.Data);
        return Task.Run(() =>
            _wasm.RenderPdf(xml, _licenseKey, fonts, images,
                _engineOpts.SrcFallback, encryptJson, callOptions?.CreatedOn, dataJson));
    }

    /// <summary>Render a <see cref="PdfDocument"/> tree to PDF bytes.</summary>
    /// <param name="document">Document tree produced by <c>Pdf.Document(…)</c>.</param>
    /// <param name="callOptions">Per-call options (timestamp).</param>
    public Task<byte[]> Render(PdfDocument document, RenderOptions? callOptions = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(document);
        var json = JsonSerializer.Serialize(document, DocumentJson.Options);
        var fonts  = _fonts.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_fonts : null;
        var images = _images.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_images : null;
        var encryptJson = BuildEncryptJson();
        return Task.Run(() =>
            _wasm.RenderTreePdf(json, _licenseKey, fonts, images,
                _engineOpts.SrcFallback, encryptJson, callOptions?.CreatedOn));
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private string? BuildEncryptJson()
    {
        if (_encrypt is null) return null;
        var p = _encrypt.Permissions;
        return JsonSerializer.Serialize(new
        {
            user_password  = _encrypt.UserPassword,
            owner_password = _encrypt.OwnerPassword,
            permissions    = new
            {
                print         = p.Print,
                modify        = p.Modify,
                copy          = p.Copy,
                annotate      = p.Annotate,
                fill_forms    = p.FillForms,
                accessibility = p.Accessibility,
                assemble      = p.Assemble,
                print_hq      = p.PrintHq,
            },
        });
    }

    private static string? BuildDataJson(object? data)
    {
        if (data is null) return null;
        return JsonSerializer.Serialize(data);
    }

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(_disposed, this);

    /// <summary>Releases the WASM runtime resources held by this engine.</summary>
    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _wasm.Dispose();
    }
}
