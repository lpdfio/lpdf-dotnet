namespace Lpdf.Engine;

/// <summary>
/// Encryption options passed to <see cref="PdfEngine.SetEncryption"/>.
/// </summary>
public sealed class EncryptOptions
{
    /// <summary>Open password shown to readers. Empty string = no open password required.</summary>
    public string UserPassword  { get; init; } = "";
    /// <summary>Owner (permissions) password. Required; must be non-empty.</summary>
    public string OwnerPassword { get; init; } = "";
    /// <summary>Permission flags applied to the document.</summary>
    public EncryptPermissions Permissions { get; init; } = new();
    /// <summary>The cipher. AES-256 unless set to <see cref="EncryptAlgorithm.Rc4128"/>.</summary>
    public EncryptAlgorithm Algorithm { get; init; } = EncryptAlgorithm.Aes256;
}
