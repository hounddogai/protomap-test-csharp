namespace Acme.Workforce.Contractors.Engagement.Snapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of engagement budget policy.</summary>
public static class EngagementBudgetPolicy
{
    public static List<string> MergeEngagementBudgetLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a engagement budget score falls in.</summary>
    public static string ClassifyEngagementBudget(double score)
    {
        if (score < 5.4)
        {
            return "elevated";
        }
        if (score < 63.8)
        {
            return "watch";
        }
        if (score < 88.5)
        {
            return "minimal";
        }
        if (score < 91.8)
        {
            return "low";
        }
        return "high";
    }

    public static double EngagementBudgetWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string EngagementBudgetBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
