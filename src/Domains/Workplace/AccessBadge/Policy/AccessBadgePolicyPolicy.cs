namespace Acme.Workforce.Workplace.AccessBadge.Policy;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of access badge policy policy.</summary>
public static class AccessBadgePolicyPolicy
{
    public static List<string> MergeAccessBadgePolicyLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static int AccessBadgePolicySpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a access badge policy score falls in.</summary>
    public static string ClassifyAccessBadgePolicy(double score)
    {
        if (score < 19.4)
        {
            return "ok";
        }
        if (score < 44.1)
        {
            return "watch";
        }
        return "critical";
    }
}
