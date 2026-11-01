using System.Security.Cryptography;
using System.Text.Json;
using Lpdf.Engine;
using Lpdf.Kit;
using Lpdf.Shared;

namespace Lpdf;

/// <summary>
/// Stateful lpdf renderer.
/// Create via <see cref="L.Engine"/>; call <see cref="Render(string, RenderOptions?)"/>
/// or <see cref="Render(PdfDocument, RenderOptions?)"/> as many times as needed.
/// </summary>
public sealed class PdfEngine : IDisposable
{
    /// <summary>Bytes of entropy sent with a request that has a password: one 256-bit key's worth.</summary>
    private const int EntropyBytes = 32;

    private          string         _licenseKey = string.Empty;
    private readonly EngineOptions  _engineOpts;
    private readonly WasmRunner     _wasm;
    private readonly Dictionary<string, byte[]> _fonts  = new();
    private readonly Dictionary<string, byte[]> _images = new();
    private readonly Dictionary<string, byte[]> _files  = new();
    private readonly Dictionary<string, byte[]> _profiles = new();
    private readonly Dictionary<string, string> _markdownStyles = new();
    private          bool           _disposed;

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
    /// Ask the engine what a license key is: valid, expired, for another product, and which
    /// license and key it is.
    /// </summary>
    /// <param name="key">
    /// The key to check, or <see langword="null"/> to check the one set on this engine.
    /// </param>
    /// <remarks>
    /// This is the engine's own verdict, reached by the same code a render runs — so
    /// <see cref="LicenseCheck.IsLicensed"/> means PDFs will come out without the attribution
    /// line here. A key the portal considers perfectly good still reads <c>unknown_key</c> in a
    /// build that does not trust the key it was signed with, which is the answer worth having.
    /// </remarks>
    public LicenseCheck CheckLicenseKey(string? key = null)
    {
        ThrowIfDisposed();
        var json = WasmRunner.CheckLicense(key ?? _licenseKey);
        return JsonSerializer.Deserialize<LicenseCheck>(json)
            ?? throw new InvalidOperationException("The engine returned no license report.");
    }

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

    /// <summary>
    /// Register the bytes of a file the document carries, for the <c>&lt;file&gt;</c> of its <c>&lt;attachments&gt;</c>
    /// whose <c>ref</c>, or with no ref whose name, is <paramref name="name"/>. The PDF carries them as they are.
    /// Call before <see cref="Render(string, RenderOptions?)"/>. Returns <c>this</c>.
    /// </summary>
    public PdfEngine LoadFile(string name, byte[] bytes)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(bytes);
        _files[name] = bytes;
        return this;
    }

    /// <summary>
    /// Register the bytes of an ICC color profile, for the <c>&lt;profile&gt;</c> of the document's <c>&lt;assets&gt;</c>
    /// whose <c>ref</c>, or with no ref whose name, is <paramref name="name"/>: the press profile a PDF/X document names
    /// in its output. Call before <see cref="Render(string, RenderOptions?)"/>. Returns <c>this</c>.
    /// </summary>
    public PdfEngine LoadProfile(string name, byte[] bytes)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(bytes);
        _profiles[name] = bytes;
        return this;
    }

    /// <summary>
    /// Register the text of a file of Markdown rules, the root of which is a <c>&lt;markdown-styles&gt;</c>, for the
    /// <c>src</c> that names it in <c>&lt;markdown-styles src="…"&gt;</c>. Call before
    /// <see cref="Render(string, RenderOptions?)"/>. Returns <c>this</c>.
    /// </summary>
    public PdfEngine LoadMarkdownStyles(string src, string text)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(text);
        _markdownStyles[src] = text;
        return this;
    }

    /// <summary>
    /// Render an lpdf XML string to PDF bytes, on the caller's thread. To render on another thread, write
    /// <c>await Task.Run(() =&gt; engine.Render(xml))</c>.
    /// </summary>
    /// <param name="xml">Full lpdf XML document string.</param>
    /// <param name="callOptions">Per-call options (data, timestamp, passwords).</param>
    public byte[] Render(string xml, RenderOptions? callOptions = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(xml);
        var fonts  = _fonts.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_fonts : null;
        var images = _images.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_images : null;
        var encryption = BuildEncryption(callOptions);
        var dataJson   = BuildDataJson(callOptions?.Data);
        var styles = _markdownStyles.Count > 0 ? _markdownStyles : null;
        return _wasm.RenderPdf(xml, _licenseKey, fonts, images,
            _engineOpts.SrcFallback, encryption, callOptions?.CreatedOn, dataJson, styles, _files, _profiles);
    }

    /// <summary>
    /// Render a <see cref="PdfDocument"/> tree to PDF bytes, on the caller's thread. To render on another thread,
    /// write <c>await Task.Run(() =&gt; engine.Render(document))</c>.
    /// </summary>
    /// <param name="document">Document tree produced by <c>Pdf.Document(…)</c>.</param>
    /// <param name="callOptions">Per-call options (timestamp, passwords).</param>
    public byte[] Render(PdfDocument document, RenderOptions? callOptions = null)
    {
        ThrowIfDisposed();
        ArgumentNullException.ThrowIfNull(document);
        var json = JsonSerializer.Serialize(document, DocumentJson.Options);
        var fonts  = _fonts.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_fonts : null;
        var images = _images.Count > 0 ? (IReadOnlyDictionary<string, byte[]>)_images : null;
        var encryption = BuildEncryption(callOptions);
        var styles = _markdownStyles.Count > 0 ? _markdownStyles : null;
        return _wasm.RenderTreePdf(json, _licenseKey, fonts, images,
            _engineOpts.SrcFallback, encryption, callOptions?.CreatedOn, styles, _files, _profiles);
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    /// <summary>
    /// The passwords and <c>skip_encryption</c> of the render, as given, and with a password, entropy for AES-256:
    /// random bytes the engine does not make, from the operating system's secure generator, fresh for each render.
    /// </summary>
    private static EncryptionRequest BuildEncryption(RenderOptions? options)
    {
        var hasPassword = options?.UserPassword is not null || options?.OwnerPassword is not null;
        var entropy = hasPassword ? Convert.ToBase64String(RandomNumberGenerator.GetBytes(EntropyBytes)) : null;
        return new EncryptionRequest(options?.UserPassword, options?.OwnerPassword, options?.SkipEncryption ?? false, entropy);
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
