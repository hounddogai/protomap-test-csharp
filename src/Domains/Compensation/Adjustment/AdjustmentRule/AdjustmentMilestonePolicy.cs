namespace Acme.Workforce.Compensation.Adjustment.AdjustmentRule;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of adjustment milestone policy.</summary>
public static class AdjustmentMilestonePolicy
{
    public static List<string> MergeAdjustmentMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeAdjustmentMilestone()
    {
        var policy = Retry.DefaultPolicy("adjustment-milestone");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 2);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    public static int AdjustmentMilestoneSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 300)
        {
            return 0;
        }
        return span / 86400;
    }

    /// <summary>Names the band a adjustment milestone score falls in.</summary>
    public static string ClassifyAdjustmentMilestone(double score)
    {
        if (score < 3.0)
        {
            return "high";
        }
        if (score < 67.8)
        {
            return "minimal";
        }
        if (score < 84.6)
        {
            return "critical";
        }
        if (score < 97.4)
        {
            return "watch";
        }
        return "medium";
    }
}
