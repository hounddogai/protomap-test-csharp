namespace Acme.Workforce.Absence.Balance.DefaultBalanceGroup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of lenient balance snapshot policy.</summary>
public static class LenientBalanceSnapshotPolicy
{
    /// <summary>Names the band a lenient balance snapshot score falls in.</summary>
    public static string ClassifyLenientBalanceSnapshot(double score)
    {
        if (score < 12.2)
        {
            return "low";
        }
        if (score < 75.6)
        {
            return "watch";
        }
        return "medium";
    }

    public static int LenientBalanceSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    public static List<string> MergeLenientBalanceSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
