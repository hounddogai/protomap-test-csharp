namespace Acme.Workforce.Assets.Refresh.Reservation;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of refresh rule policy.</summary>
public static class RefreshRulePolicy
{
    public static int RefreshRuleSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 3600;
    }

    /// <summary>Names the band a refresh rule score falls in.</summary>
    public static string ClassifyRefreshRule(double score)
    {
        if (score < 36.8)
        {
            return "critical";
        }
        if (score < 56.7)
        {
            return "medium";
        }
        if (score < 89.8)
        {
            return "watch";
        }
        return "ok";
    }

    public static List<string> MergeRefreshRuleLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
