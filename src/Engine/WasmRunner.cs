using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using Wasmtime;
using WasmEngine = Wasmtime.Engine;
using WasmModule = Wasmtime.Module;

namespace Lpdf.Engine;

/// <summary>
/// Thin wrapper around the Wasmtime.NET engine that loads the embedded
/// <c>lpdf.wasm</c> WASI binary and exposes <c>render</c>/<c>render_tree</c>
/// as synchronous string-in / string-out calls, and <c>render_pdf</c> /
/// <c>render_tree_pdf</c> as calls that return raw PDF bytes.
/// </summary>
internal sealed class WasmRunner : IDisposable
{
    // One thread advances the engine's epoch at this pace, and a render's deadline is its timeout in ticks, plus one:
    // the render starts between two ticks, and without it would stop up to a tick before its limit. The epoch belongs
    // to the engine, not to a store, so a timer per render would stop every other render too.
    private static readonly TimeSpan Tick = TimeSpan.FromMilliseconds(100);
    private const ulong TicksPerSecond = 10;

    // The deadline of a render with no limit. A store's deadline is 0 until it is set, which stops it at the first tick.
    private const ulong NoDeadline = 1UL << 62;

    // The engine and module are expensive to create and thread-safe — share them.
    private static readonly WasmEngine   _engine;
    private static readonly WasmModule  _lpdfModule;
    private static readonly Linker      _linker;

    static WasmRunner()
    {
        _engine = new WasmEngine(new Config().WithEpochInterruption(true));
        _lpdfModule = WasmModule.FromBytes(_engine, "lpdf", LoadWasmBytes());
        _linker = new Linker(_engine);
        _linker.DefineWasi();
        new Thread(AdvanceEpoch) { IsBackground = true, Name = "lpdf-epoch" }.Start();
    }

    private readonly WasmModule _module;
    private readonly int        _timeout;
    private bool _disposed;

    /// <param name="timeout">
    /// The seconds a call may run before it stops with an <see cref="EngineException"/>, or 0 for no limit.
    /// </param>
    public WasmRunner(int timeout) : this(_lpdfModule, timeout) { }

    private WasmRunner(WasmModule module, int timeout)
    {
        _module  = module;
        _timeout = timeout;
    }

    /// <summary>
    /// A runner of a module written in the WebAssembly text format in place of the engine, compiled on the engine's
    /// Wasmtime engine: for the tests, a module that runs as long as they need.
    /// </summary>
    internal static WasmRunner ForModuleText(string wat, int timeout)
        => new(WasmModule.FromText(_engine, "stand-in", wat), timeout);

    // ── Public ────────────────────────────────────────────────────────────────

    /// <summary>Render an lpdf XML document and return the RenderTree JSON string.</summary>
    public string Render(string xml, string licenseKey)
        => Invoke("render", xml, licenseKey);

    /// <summary>Render an lpdf JSON tree and return the RenderTree JSON string.</summary>
    public string RenderTree(string json, string licenseKey)
        => Invoke("render_tree", json, licenseKey);

    /// <summary>
    /// Render an lpdf XML document to raw PDF bytes.
    /// Custom fonts are base64-encoded into the request envelope.
    /// If <paramref name="srcFallback"/> is provided, a preliminary <c>render</c>
    /// call is made to discover font <c>src</c> paths so they can be resolved
    /// before the PDF render call.
    /// </summary>
    public byte[] RenderPdf(
        string xml,
        string licenseKey,
        IReadOnlyDictionary<string, byte[]>? fontBytes,
        IReadOnlyDictionary<string, byte[]>? imageBytes,
        Func<string, byte[]>?                srcFallback,
        EncryptionRequest?                   encryption = null,
        string?                              createdOn = null,
        string?                              dataJson = null,
        IReadOnlyDictionary<string, string>? markdownStyles = null,
        IReadOnlyDictionary<string, byte[]>? fileBytes = null,
        IReadOnlyDictionary<string, byte[]>? profileBytes = null)
        => InvokeRenderPdf("render_pdf", xml, licenseKey, fontBytes, imageBytes, srcFallback, isTree: false, encryption, createdOn, dataJson, markdownStyles, fileBytes, profileBytes);

    /// <summary>
    /// Render an lpdf JSON document tree to raw PDF bytes.
    /// </summary>
    public byte[] RenderTreePdf(
        string json,
        string licenseKey,
        IReadOnlyDictionary<string, byte[]>? fontBytes,
        IReadOnlyDictionary<string, byte[]>? imageBytes,
        Func<string, byte[]>?                srcFallback,
        EncryptionRequest?                   encryption = null,
        string?                              createdOn = null,
        IReadOnlyDictionary<string, string>? markdownStyles = null,
        IReadOnlyDictionary<string, byte[]>? fileBytes = null,
        IReadOnlyDictionary<string, byte[]>? profileBytes = null)
        => InvokeRenderPdf("render_tree_pdf", json, licenseKey, fontBytes, imageBytes, srcFallback, isTree: true, encryption, createdOn, null, markdownStyles, fileBytes, profileBytes);

    /// <summary>
    /// Convert a JSON kit-tree (produced by <c>LpdfKit</c>) to an lpdf XML string.
    /// </summary>
    public string KitToXml(string json)
    {
        var requestObj  = new JsonObject { ["method"] = "kit_to_xml", ["input"] = json };
        var requestBytes = Encoding.UTF8.GetBytes(requestObj.ToJsonString());
        var responseJson = RunWasm(requestBytes);

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out var errEl))
            throw new EngineException(errEl.GetString() ?? "Unknown kit_to_xml error.");

        if (!root.TryGetProperty("xml", out var xmlEl))
            throw new InvalidOperationException("WASM kit_to_xml response missing 'xml' field.");

        return xmlEl.GetString()
            ?? throw new InvalidOperationException("WASM kit_to_xml 'xml' field is null.");
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private byte[] InvokeRenderPdf(
        string method,
        string input,
        string licenseKey,
        IReadOnlyDictionary<string, byte[]>? fontBytes,
        IReadOnlyDictionary<string, byte[]>? imageBytes,
        Func<string, byte[]>?                srcFallback,
        bool                                 isTree,
        EncryptionRequest?                   encryption = null,
        string?                              createdOn = null,
        string?                              dataJson = null,
        IReadOnlyDictionary<string, string>? markdownStyles = null,
        IReadOnlyDictionary<string, byte[]>? fileBytes = null,
        IReadOnlyDictionary<string, byte[]>? profileBytes = null)
    {
        // An XML document is parsed once, for the src of every file it names.
        var xml    = isTree ? null : ParseXml(input);
        var fonts  = ResolveAllFonts(
            isTree ? ExtractAssetSrcsFromJson(input, "fonts") : ExtractSrcsFromXml(xml, "assets", "font"), fontBytes, srcFallback);
        var images = ResolveAllImages(
            isTree ? ExtractAssetSrcsFromJson(input, "images") : ExtractSrcsFromXml(xml, "assets", "image"), imageBytes, srcFallback);
        var styles = ResolveAllMarkdownStyles(
            isTree ? ExtractMarkdownStylesSrcsFromJson(input) : ExtractMarkdownStylesSrcsFromXml(xml), markdownStyles, srcFallback);
        var files  = ResolveAllFiles(
            isTree ? ExtractFileSrcsFromJson(input) : ExtractSrcsFromXml(xml, "attachments", "file"), fileBytes, srcFallback);
        var profiles = ResolveAllProfiles(
            isTree ? ExtractAssetSrcsFromJson(input, "profiles") : ExtractSrcsFromXml(xml, "assets", "profile"), profileBytes, srcFallback);

        var fontsNode = new JsonObject();
        foreach (var (name, bytes) in fonts)
            fontsNode[name] = Convert.ToBase64String(bytes);

        var requestObj = new JsonObject
        {
            ["method"]  = method,
            ["key"]     = licenseKey,
            ["input"]   = input,
            ["fonts"]   = fontsNode,
            // See NowUnix: without it the engine cannot check the key's expiry at all.
            ["now"]     = NowUnix(),
        };

        if (images is { Count: > 0 })
        {
            var imagesNode = new JsonObject();
            foreach (var (name, bytes) in images)
                imagesNode[name] = Convert.ToBase64String(bytes);
            requestObj["images"] = imagesNode;
        }

        if (files.Count > 0)
        {
            var filesNode = new JsonObject();
            foreach (var (name, bytes) in files)
                filesNode[name] = Convert.ToBase64String(bytes);
            requestObj["files"] = filesNode;
        }

        if (profiles.Count > 0)
        {
            var profilesNode = new JsonObject();
            foreach (var (name, bytes) in profiles)
                profilesNode[name] = Convert.ToBase64String(bytes);
            requestObj["profiles"] = profilesNode;
        }

        if (styles.Count > 0)
        {
            var stylesNode = new JsonObject();
            foreach (var (src, text) in styles)
                stylesNode[src] = text;
            requestObj["markdown_styles"] = stylesNode;
        }

        if (encryption is not null)
        {
            if (encryption.UserPassword is not null)
                requestObj["user_password"] = encryption.UserPassword;
            if (encryption.OwnerPassword is not null)
                requestObj["owner_password"] = encryption.OwnerPassword;
            if (encryption.SkipEncryption)
                requestObj["skip_encryption"] = true;
            if (encryption.Entropy is not null)
                requestObj["entropy"] = encryption.Entropy;
        }

        if (createdOn is not null)
            requestObj["created_on"] = createdOn;

        if (dataJson is not null)
            requestObj["data"] = JsonNode.Parse(dataJson);

        var requestBytes = Encoding.UTF8.GetBytes(requestObj.ToJsonString());
        var responseJson = RunWasm(requestBytes);

        using var doc = JsonDocument.Parse(responseJson);
        var root = doc.RootElement;

        if (root.TryGetProperty("error", out var errEl))
            throw new EngineException(errEl.GetString() ?? "Unknown render error.");

        if (!root.TryGetProperty("pdf", out var pdfEl))
            throw new InvalidOperationException("WASM render_pdf response missing 'pdf' field.");

        return Convert.FromBase64String(pdfEl.GetString()
            ?? throw new InvalidOperationException("WASM render_pdf 'pdf' field is null."));
    }

    private static IReadOnlyDictionary<string, byte[]> ResolveAllFonts(
        Dictionary<string, string>           srcs,
        IReadOnlyDictionary<string, byte[]>? fontBytes,
        Func<string, byte[]>?                srcFallback)
    {
        if (srcFallback is null)
            return fontBytes ?? new Dictionary<string, byte[]>();

        var merged = new Dictionary<string, byte[]>(fontBytes ?? new Dictionary<string, byte[]>());
        foreach (var (name, src) in srcs)
        {
            if (merged.ContainsKey(name)) continue;
            try   { merged[name] = srcFallback(src); }
            catch { /* skip unresolvable fonts */ }
        }
        return merged;
    }

    private static IReadOnlyDictionary<string, byte[]> ResolveAllImages(
        Dictionary<string, string>           srcs,
        IReadOnlyDictionary<string, byte[]>? imageBytes,
        Func<string, byte[]>?                srcFallback)
    {
        var merged = new Dictionary<string, byte[]>(imageBytes ?? new Dictionary<string, byte[]>());
        if (srcFallback is null) return merged;

        foreach (var (name, src) in srcs)
        {
            if (merged.ContainsKey(name)) continue;
            try   { merged[name] = srcFallback(src); }
            catch { /* skip unresolvable images */ }
        }
        return merged;
    }

    /// <summary>
    /// The files the document carries: the ones loaded, and the ones the <paramref name="srcFallback"/> reads from the
    /// <c>src</c> of a file that wasn't. A <c>src</c> that can't be read, or that there is no fallback to read, is an
    /// error that names the file, so the engine's "no bytes" never stands for a path that was given.
    /// </summary>
    private static Dictionary<string, byte[]> ResolveAllFiles(
        Dictionary<string, string>           srcs,
        IReadOnlyDictionary<string, byte[]>? fileBytes,
        Func<string, byte[]>?                srcFallback)
    {
        var merged = new Dictionary<string, byte[]>(fileBytes ?? new Dictionary<string, byte[]>(), StringComparer.Ordinal);
        foreach (var (key, src) in srcs)
        {
            if (merged.ContainsKey(key)) continue;
            if (srcFallback is null)
                throw new EngineException(
                    $"file '{key}' of <attachments> has the src '{src}', and the engine has no SrcFallback to read it: " +
                    $"create it with new EngineOptions {{ SrcFallback = File.ReadAllBytes }}, or load the file with LoadFile(\"{key}\", bytes)");
            try
            {
                merged[key] = srcFallback(src);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                throw new EngineException($"file '{key}' of <attachments>: cannot read its src '{src}': {e.Message}");
            }
        }
        return merged;
    }

    /// <summary>
    /// The color profiles the document declares: the ones loaded, and the ones the <paramref name="srcFallback"/> reads
    /// from the <c>src</c> of a profile that wasn't. A <c>src</c> that can't be read, or that there is no fallback to
    /// read, is an error that names the profile, as for a file the document carries.
    /// </summary>
    private static Dictionary<string, byte[]> ResolveAllProfiles(
        Dictionary<string, string>           srcs,
        IReadOnlyDictionary<string, byte[]>? profileBytes,
        Func<string, byte[]>?                srcFallback)
    {
        var merged = new Dictionary<string, byte[]>(profileBytes ?? new Dictionary<string, byte[]>(), StringComparer.Ordinal);
        foreach (var (key, src) in srcs)
        {
            if (merged.ContainsKey(key)) continue;
            if (srcFallback is null)
                throw new EngineException(
                    $"profile '{key}' of <assets> has the src '{src}', and the engine has no SrcFallback to read it: " +
                    $"create it with new EngineOptions {{ SrcFallback = File.ReadAllBytes }}, or load the profile with LoadProfile(\"{key}\", bytes)");
            try
            {
                merged[key] = srcFallback(src);
            }
            catch (Exception e) when (e is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
            {
                throw new EngineException($"profile '{key}' of <assets>: cannot read its src '{src}': {e.Message}");
            }
        }
        return merged;
    }

    /// <summary>
    /// An XML document read with an XML parser, as the engine reads it, for the <c>src</c> of the files it names:
    /// entities decoded, and what is in a comment or CDATA left out. XML that doesn't parse is <c>null</c> and names no
    /// file here; the engine says what is wrong with it.
    /// </summary>
    private static XDocument? ParseXml(string xml)
    {
        try
        {
            // The default settings refuse a DTD, so no entity of the document reaches outside it.
            return XDocument.Parse(xml);
        }
        catch (XmlException)
        {
            return null;
        }
    }

    /// <summary>
    /// The <c>ref ?? name → src</c> pairs of the <paramref name="element"/> elements of each
    /// <paramref name="parent"/> of a document: the <c>font</c>, <c>image</c> and <c>profile</c> of its <c>assets</c>,
    /// or the <c>file</c> of its <c>attachments</c>.
    /// </summary>
    private static Dictionary<string, string> ExtractSrcsFromXml(XDocument? document, string parent, string element)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        if (document is null) return result;
        foreach (var declared in document.Descendants(element).Where(e => e.Parent?.Name.LocalName == parent))
        {
            var key = (string?)declared.Attribute("ref") ?? (string?)declared.Attribute("name");
            var src = (string?)declared.Attribute("src");
            if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(src))
                result[key] = src;
        }
        return result;
    }

    /// <summary>The <c>ref ?? name → src</c> pairs of the files a document tree carries in its <c>attachments</c>.</summary>
    private static Dictionary<string, string> ExtractFileSrcsFromJson(string json)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("attrs", out var attrs)) return result;
            if (!attrs.TryGetProperty("attachments", out var files)) return result;
            foreach (var file in files.EnumerateArray())
            {
                if (!file.TryGetProperty("src", out var srcElement) || srcElement.GetString() is not { } src) continue;
                var name = file.GetProperty("name").GetString();
                var key  = file.TryGetProperty("ref", out var refElement) ? refElement.GetString() ?? name : name;
                if (key is not null) result[key] = src;
            }
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            // A tree that is not shaped like a document carries no files to read; the engine says what is wrong.
        }
        return result;
    }

    /// <summary>
    /// The files of Markdown rules a document names with a <c>src</c>: the ones loaded, and the ones the
    /// <paramref name="srcFallback"/> reads. A file that can't be read is left for the engine to report, naming the
    /// <c>src</c>.
    /// </summary>
    private static Dictionary<string, string> ResolveAllMarkdownStyles(
        List<string>                         srcs,
        IReadOnlyDictionary<string, string>? markdownStyles,
        Func<string, byte[]>?                srcFallback)
    {
        var merged = new Dictionary<string, string>(markdownStyles ?? new Dictionary<string, string>(), StringComparer.Ordinal);
        if (srcFallback is null) return merged;

        foreach (var src in srcs)
        {
            if (merged.ContainsKey(src)) continue;
            try   { merged[src] = Encoding.UTF8.GetString(srcFallback(src)); }
            catch { /* the engine says that the file was not loaded */ }
        }
        return merged;
    }

    /// <summary>The <c>src</c> of the <c>markdown-styles</c> of each <c>markdown</c> of a document.</summary>
    private static List<string> ExtractMarkdownStylesSrcsFromXml(XDocument? document)
    {
        if (document is null) return [];
        return document.Descendants("markdown-styles")
            .Where(e => e.Parent?.Name.LocalName == "markdown")
            .Select(e => (string?)e.Attribute("src"))
            .OfType<string>()
            .Where(src => src.Length > 0)
            .ToList();
    }

    /// <summary>The <c>src</c> of each <c>markdown-styles</c> in the markdown of any section of a document tree.</summary>
    private static List<string> ExtractMarkdownStylesSrcsFromJson(string json)
    {
        var result = new List<string>();
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("nodes", out var sections)) return result;
            foreach (var section in sections.EnumerateArray())
            {
                if (!section.TryGetProperty("nodes", out var children)) continue;
                foreach (var child in children.EnumerateArray())
                {
                    if (child.GetProperty("type").GetString() != "markdown") continue;
                    foreach (var part in child.GetProperty("nodes").EnumerateArray())
                    {
                        if (part.GetProperty("type").GetString() != "markdown-styles") continue;
                        if (part.GetProperty("attrs").TryGetProperty("src", out var src) && src.GetString() is { Length: > 0 } text)
                            result.Add(text);
                    }
                }
            }
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            // A tree that is not shaped like a document names no files of rules.
        }
        return result;
    }

    /// <summary>
    /// The <c>ref ?? name → src</c> pairs of the fonts, images or color profiles a document tree declares in its
    /// <c>assets</c>. <paramref name="kind"/> is <c>fonts</c>, <c>images</c> or <c>profiles</c>.
    /// </summary>
    private static Dictionary<string, string> ExtractAssetSrcsFromJson(string json, string kind)
    {
        var result = new Dictionary<string, string>(StringComparer.Ordinal);
        try
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("attrs", out var attrs)) return result;
            if (!attrs.TryGetProperty("assets", out var assets)) return result;
            if (!assets.TryGetProperty(kind, out var declared)) return result;
            foreach (var asset in declared.EnumerateArray())
            {
                if (!asset.TryGetProperty("src", out var srcElement) || srcElement.GetString() is not { } src) continue;
                var name = asset.GetProperty("name").GetString();
                var key  = asset.TryGetProperty("ref", out var refElement) ? refElement.GetString() ?? name : name;
                if (key is not null) result[key] = src;
            }
        }
        catch (Exception e) when (e is JsonException or InvalidOperationException or KeyNotFoundException)
        {
            // A tree that is not shaped like a document declares no assets to read.
        }
        return result;
    }

    private string Invoke(string method, string input, string key)
    {
        var request = JsonSerializer.Serialize(new { method, key, input, now = NowUnix() });
        return RunWasm(Encoding.UTF8.GetBytes(request));
    }

    /// <summary>What this build of the engine makes of <paramref name="licenseKey"/>, as JSON.</summary>
    public string CheckLicense(string licenseKey)
    {
        var request = JsonSerializer.Serialize(new { method = "check_license", key = licenseKey, now = NowUnix() });
        return RunWasm(Encoding.UTF8.GetBytes(request));
    }

    /// <summary>
    /// The clock the engine checks a key's expiry against.
    /// </summary>
    /// <remarks>
    /// The engine is WebAssembly and has no clock of its own, so a request that omits this has
    /// its expiry check skipped entirely — which is how an expired key used to render without
    /// the attribution line. Sent on every call rather than only when a key is present: the
    /// engine ignores it in free mode.
    /// </remarks>
    private static long NowUnix() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    private string RunWasm(byte[] requestBytes)
    {
        var inputPath  = Path.GetTempFileName();
        var outputPath = Path.GetTempFileName();
        try
        {
            File.WriteAllBytes(inputPath, requestBytes);

            var wasiConfig = new WasiConfiguration()
                .WithStandardInput(inputPath)
                .WithStandardOutput(outputPath);

            using (var store = new Store(_engine))
            {
                store.SetWasiConfiguration(wasiConfig);
                store.SetEpochDeadline(_timeout > 0 ? (ulong)_timeout * TicksPerSecond + 1 : NoDeadline);
                var instance = _linker.Instantiate(store, _module);
                var start = instance.GetAction("_start")
                    ?? throw new InvalidOperationException("lpdf WASM module does not export '_start'.");
                start();
            }

            return File.ReadAllText(outputPath, Encoding.UTF8);
        }
        catch (TrapException trap) when (trap.Type == TrapCode.Interrupt)
        {
            throw new EngineException($"The render timed out after {Seconds(_timeout)}.", trap);
        }
        finally
        {
            try { File.Delete(inputPath); }  catch { /* best-effort cleanup */ }
            try { File.Delete(outputPath); } catch { /* best-effort cleanup */ }
        }
    }

    private static string Seconds(int count) => count == 1 ? "1 second" : $"{count} seconds";

    /// <summary>Advances the engine's epoch every <see cref="Tick"/>, for as long as the process runs.</summary>
    private static void AdvanceEpoch()
    {
        // Each tick is due at a time of the clock, not a sleep after the last one, so the ticks don't drift: a sleep
        // ends late by the timer's resolution, 15.6 ms on Windows.
        var clock = Stopwatch.StartNew();
        var due   = TimeSpan.Zero;
        while (true)
        {
            due += Tick;
            var wait = due - clock.Elapsed;
            if (wait > TimeSpan.Zero) Thread.Sleep(wait);
            _engine.IncrementEpoch();
        }
    }

    private static byte[] LoadWasmBytes()
    {
        var asm  = Assembly.GetExecutingAssembly();
        const string resourceName = "Lpdf.wasm.lpdf.wasm";
        using var stream = asm.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException(
                $"Embedded WASM resource '{resourceName}' not found. " +
                "Run 'make wasi' first to build dist/wasi/lpdf.wasm, " +
                "then copy it to src/adapters/dotnet/wasm/lpdf.wasm.");

        using var ms = new MemoryStream((int)stream.Length);
        stream.CopyTo(ms);
        return ms.ToArray();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        // _engine, _module, _linker are shared — not disposed here.
    }
}
