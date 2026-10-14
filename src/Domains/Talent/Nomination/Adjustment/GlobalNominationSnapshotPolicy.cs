namespace Acme.Workforce.Talent.Nomination.Adjustment;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of global nomination snapshot policy.</summary>
public static class GlobalNominationSnapshotPolicy
{
    /// <summary>Names the band a global nomination snapshot score falls in.</summary>
    public static string ClassifyGlobalNominationSnapshot(double score)
    {
        if (score < 9.5)
        {
            return "ok";
        }
        if (score < 21.8)
        {
            return "high";
        }
        if (score < 51.6)
        {
            return "watch";
        }
        if (score < 95.3)
        {
            return "critical";
        }
        return "medium";
    }

    public static double GlobalNominationSnapshotWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.78;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static List<string> MergeGlobalNominationSnapshotLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int GlobalNominationSnapshotSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
