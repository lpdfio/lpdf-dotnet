// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>A file the PDF carries, under the name a reader shows. The SDK reads it from src, or it was loaded on the engine under ref or, with no ref, under its own name. A file with no bytes is an error, and so are two files with the same name.</summary>
public sealed record FileAttr
{
    /// <summary>The name a reader shows the file under and saves it as, such as invoice.xml. Each file of the document has a name of its own.</summary>
    public required string Name { get; init; }

    /// <summary>A path the SDK reads the file from, when the file was not loaded on the engine.</summary>
    public string? Src { get; init; }

    /// <summary>The key the file was loaded under on the engine, when that is not its name.</summary>
    public string? Ref { get; init; }

    /// <summary>The type of the file, such as text/xml, application/pdf or text/csv. Defaults to application/octet-stream, which says only that it is a file.</summary>
    public string? Mime { get; init; }

    /// <summary>How the file relates to the document: source, data, alternative, supplement or unspecified, the default. An e-invoice's XML is data.</summary>
    public string? Relationship { get; init; }

    /// <summary>A line about the file, which a reader shows beside its name.</summary>
    public string? Description { get; init; }
}
