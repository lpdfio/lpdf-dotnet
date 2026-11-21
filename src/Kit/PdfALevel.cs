// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>A level of PDF/A, the ISO 19005 standard for files kept for the long term: 2b keeps the look of every page, 2u adds that all text maps to Unicode, so it can be searched and copied, and 2a adds to 2u the tagged structure a screen reader reads. 3a, 3b and 3u are the same levels in part 3, which also carries files of any kind, as a Factur-X or ZUGFeRD e-invoice carries its XML.</summary>
public static class PdfALevel
{
    public const string Level2a = "2a";
    public const string Level2b = "2b";
    public const string Level2u = "2u";
    public const string Level3a = "3a";
    public const string Level3b = "3b";
    public const string Level3u = "3u";
}
