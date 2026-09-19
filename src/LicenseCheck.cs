using System.Text.Json.Serialization;

namespace Lpdf;

/// <summary>
/// What this build of the engine makes of a license key — see
/// <see cref="PdfEngine.CheckLicenseKey(string?)"/>.
/// </summary>
/// <remarks>
/// Everything but <see cref="Status"/> is populated only once the key's signature verified, so
/// an expired or wrong-version key still names its license while an unreadable one says nothing
/// further: an unverified token's contents are its author's claims, not facts.
/// </remarks>
public sealed record LicenseCheck
{
    /// <summary>
    /// One of <c>licensed</c>, <c>free</c>, <c>expired</c>, <c>version_mismatch</c>,
    /// <c>wrong_product</c>, <c>unknown_key</c>, <c>bad_signature</c> or <c>malformed</c>.
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; init; } = "malformed";

    /// <summary>Which Codesense product the key was issued for.</summary>
    [JsonPropertyName("product")]
    public string? Product { get; init; }

    /// <summary><c>community</c>, <c>professional</c> or <c>enterprise</c>.</summary>
    [JsonPropertyName("tier")]
    public string? Tier { get; init; }

    /// <summary>
    /// When the key stops validating, ISO 8601 UTC. Null on a version-locked key, which has no
    /// date at all rather than one far in the future.
    /// </summary>
    [JsonPropertyName("expires")]
    public string? Expires { get; init; }

    /// <summary>The license number, as the customer reads it: <c>L-7K3M9Q</c>.</summary>
    [JsonPropertyName("license")]
    public string? License { get; init; }

    /// <summary>Which key this is on its license — 1, 2, 3 in issue order.</summary>
    [JsonPropertyName("key")]
    public int? Key { get; init; }

    /// <summary>True only for a key this engine will render with, unwatermarked.</summary>
    [JsonIgnore]
    public bool IsLicensed => Status == "licensed";
}
