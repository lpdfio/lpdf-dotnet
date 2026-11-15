namespace Lpdf.Engine;

/// <summary>One file for <see cref="PdfEngine.Merge"/>: its name, which every error about the file names, and its bytes.</summary>
public sealed record MergeSource
{
    /// <summary>The file's name, such as <c>contract-b.pdf</c>. Every error about the file names it.</summary>
    public required string Name { get; init; }

    /// <summary>The PDF's bytes.</summary>
    public required byte[] Pdf { get; init; }
}
