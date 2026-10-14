namespace Acme.Workforce.Workplace.AccessBadge.Rollup;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of access badge request policy.</summary>
public static class AccessBadgeRequestPolicy
{
    /// <summary>Names the band a access badge request score falls in.</summary>
    public static string ClassifyAccessBadgeRequest(double score)
    {
        if (score < 26.4)
        {
            return "minimal";
        }
        if (score < 84.2)
        {
            return "critical";
        }
        if (score < 94.6)
        {
            return "watch";
        }
        return "high";
    }

    public static string AccessBadgeRequestBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }

    public static List<string> MergeAccessBadgeRequestLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
