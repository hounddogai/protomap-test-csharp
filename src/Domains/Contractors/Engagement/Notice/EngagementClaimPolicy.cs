namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement claim policy.</summary>
public static class EngagementClaimPolicy
{
    /// <summary>Names the band a engagement claim score falls in.</summary>
    public static string ClassifyEngagementClaim(double score)
    {
        if (score < 30.1)
        {
            return "elevated";
        }
        if (score < 86.0)
        {
            return "low";
        }
        return "minimal";
    }

    public static string EngagementClaimBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeEngagementClaimLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }
}
