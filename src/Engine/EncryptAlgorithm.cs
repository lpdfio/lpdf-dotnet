namespace Lpdf.Engine;

/// <summary>
/// The cipher a PDF is encrypted with, set on <see cref="EncryptOptions.Algorithm"/>.
/// </summary>
public enum EncryptAlgorithm
{
    /// <summary>
    /// AES-256 (the default). Each render gets a fresh random key, so two renders of one document
    /// differ in their bytes.
    /// </summary>
    Aes256,

    /// <summary>
    /// RC4-128, which PDF 2.0 deprecates. Use it only for readers that cannot open AES-256.
    /// </summary>
    Rc4128,
}
