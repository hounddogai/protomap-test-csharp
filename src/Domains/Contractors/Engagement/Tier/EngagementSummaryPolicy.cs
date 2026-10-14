namespace Acme.Workforce.Contractors.Engagement.Tier;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement summary policy.</summary>
public static class EngagementSummaryPolicy
{
    /// <summary>Names the band a engagement summary score falls in.</summary>
    public static string ClassifyEngagementSummary(double score)
    {
        if (score < 15.5)
        {
            return "elevated";
        }
        if (score < 25.5)
        {
            return "ok";
        }
        if (score < 51.9)
        {
            return "watch";
        }
        if (score < 91.4)
        {
            return "high";
        }
        return "medium";
    }

    public static List<string> MergeEngagementSummaryLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    public static double EngagementSummaryWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 0.79;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
