// Generated from lpdf.xsd by scripts/gen-sdk-api.mjs.
// Do not edit: change the schema and run `make gen-sdk-api`.
namespace Lpdf.Kit;

#pragma warning disable CS1591
/// <summary>What a reader of an encrypted PDF may do without the owner password: all of what follows, or print (at full quality), modify, copy, annotate, fill in forms, extract text for accessibility tools, and assemble (insert, delete and rotate pages).</summary>
public static class Permission
{
    public const string All = "all";
    public const string Print = "print";
    public const string Modify = "modify";
    public const string Copy = "copy";
    public const string Annotate = "annotate";
    public const string FillForms = "fill-forms";
    public const string Accessibility = "accessibility";
    public const string Assemble = "assemble";
}
