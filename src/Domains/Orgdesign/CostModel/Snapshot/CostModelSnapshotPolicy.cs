namespace Acme.Workforce.Orgdesign.CostModel.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of cost model snapshot policy.</summary>
public static class CostModelSnapshotPolicy
{
    public static List<string> MergeCostModelSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int CostModelSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 60;
    }

    /// <summary>Puts a cost model snapshot code in its canonical CMS- form.</summary>
    public static string NormalizeCostModelSnapshotCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("CMS-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"CMS-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
