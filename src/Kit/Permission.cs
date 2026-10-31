// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>The words of the <c>allow</c> attribute of the output, separated by spaces: what a reader of an encrypted PDF may do.</summary>
public static class Permission
{
    public const string Print = "print";
    public const string Modify = "modify";
    public const string Copy = "copy";
    public const string Annotate = "annotate";
    public const string FillForms = "fill-forms";
    public const string Accessibility = "accessibility";
    public const string Assemble = "assemble";
}
