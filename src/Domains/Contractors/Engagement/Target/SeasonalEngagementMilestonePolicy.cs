namespace Acme.Workforce.Contractors.Engagement.Target;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of seasonal engagement milestone policy.</summary>
public static class SeasonalEngagementMilestonePolicy
{
    public static List<string> MergeSeasonalEngagementMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalEngagementMilestone()
    {
        var policy = Retry.DefaultPolicy("seasonal-engagement-milestone");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 5);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static double SeasonalEngagementMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 1.16;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a seasonal engagement milestone score falls in.</summary>
    public static string ClassifySeasonalEngagementMilestone(double score)
    {
        if (score < 88.3)
        {
            return "elevated";
        }
        if (score < 94.9)
        {
            return "medium";
        }
        if (score < 96.9)
        {
            return "critical";
        }
        return "watch";
    }
}
