using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace Lpdf.Tests;

/// <summary>
/// Shared snapshot-test helpers: path resolution, SHA-256 hashing, and
/// compare-or-update logic. Used by <see cref="SnapshotTests"/>.
/// </summary>
internal static class SnapshotHelper
{
    internal static readonly string Root      = FindRoot();
    internal static readonly string Fixtures  = Path.Combine(Root, "test", "fixtures");
    internal static readonly string Snapshots = Path.Combine(Root, "test", "snapshots");
    internal static readonly bool   Update    =
        Environment.GetEnvironmentVariable("UPDATE_SNAPSHOTS") == "1";
    internal static readonly bool   HAS_FIXTURES =
        Directory.Exists(Fixtures) && Directory.GetFiles(Fixtures, "*.xml").Length > 0;

    /// <summary>
    /// Asserts that <paramref name="bytes"/> starts with the <c>%PDF-</c> header,
    /// then compares (or updates) the SHA-256 hash against the stored snapshot
    /// for <paramref name="name"/>.
    /// </summary>
    internal static void CompareOrUpdate(string name, byte[] bytes)
    {
        Assert.True(
            bytes[..5].SequenceEqual(Encoding.ASCII.GetBytes("%PDF-")),
            "Output must start with %PDF-");

        var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
        var snap = Path.Combine(Snapshots, $"{name}.pdf.sha256");

        if (Update)
        {
            File.WriteAllText(snap, hash);
        }
        else
        {
            var stored = File.ReadAllText(snap).Trim();
            Assert.Equal(stored, hash);
        }
    }

    private static string FindRoot()
    {
        // Walk up from the test assembly looking for Cargo.toml (monorepo root).
        // Fall back to Lpdf.sln (standalone adapter repo root) when not inside
        // the monorepo — e.g. in a CI checkout of the adapter repo alone.
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        string? slnRoot = null;
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "Cargo.toml")))
                return dir.FullName;
            if (slnRoot == null && File.Exists(Path.Combine(dir.FullName, "Lpdf.sln")))
                slnRoot = dir.FullName;
            dir = dir.Parent;
        }
        return slnRoot
            ?? throw new InvalidOperationException(
                "Could not locate project root (Cargo.toml or Lpdf.sln not found).");
    }
}
