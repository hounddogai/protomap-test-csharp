namespace Acme.Workforce.Orgdesign.Reorg.ReorgProfile;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adaptive reorg snapshot policy.</summary>
public static class AdaptiveReorgSnapshotPolicy
{
    /// <summary>Puts a adaptive reorg snapshot code in its canonical ARS- form.</summary>
    public static string NormalizeAdaptiveReorgSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("ARS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"ARS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static List<string> MergeAdaptiveReorgSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
