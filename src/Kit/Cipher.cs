// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>The cipher a PDF is encrypted with: AES-256, or RC4-128 for readers that cannot open AES-256. PDF 2.0 deprecates RC4-128.</summary>
public static class Cipher
{
    public const string Aes256 = "aes-256";
    public const string Rc4128 = "rc4-128";
}
