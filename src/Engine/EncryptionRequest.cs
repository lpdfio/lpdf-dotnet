namespace Lpdf.Engine;

/// <summary>
/// What a render request says about the encryption the document asks for in its <c>output</c>: the passwords,
/// <c>skip_encryption</c>, and the entropy, base64, when there is a password.
/// </summary>
internal sealed record EncryptionRequest(string? UserPassword, string? OwnerPassword, bool SkipEncryption, string? Entropy);
