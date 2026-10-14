namespace Acme.Workforce.Contractors.Engagement.EngagementSnapshot;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of scheduled engagement review policy.</summary>
public static class ScheduledEngagementReviewPolicy
{
    public static string ScheduledEngagementReviewBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    public static List<string> MergeScheduledEngagementReviewLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Names the band a scheduled engagement review score falls in.</summary>
    public static string ClassifyScheduledEngagementReview(double score)
    {
        if (score < 30.3)
        {
            return "high";
        }
        if (score < 73.4)
        {
            return "medium";
        }
        if (score < 93.2)
        {
            return "low";
        }
        return "watch";
    }

    public static double ScheduledEngagementReviewWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 1.08;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
