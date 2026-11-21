// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>How a file the PDF carries relates to the document: source, the file the document was made from; data, the data its pages show, such as the XML of an e-invoice; alternative, the same content in another form; supplement, more than the pages show; or unspecified.</summary>
public static class FileRelationship
{
    public const string Source = "source";
    public const string Data = "data";
    public const string Alternative = "alternative";
    public const string Supplement = "supplement";
    public const string Unspecified = "unspecified";
}
