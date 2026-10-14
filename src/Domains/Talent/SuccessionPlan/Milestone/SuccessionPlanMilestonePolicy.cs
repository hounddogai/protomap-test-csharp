namespace Acme.Workforce.Talent.SuccessionPlan.Milestone;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of succession plan milestone policy.</summary>
public static class SuccessionPlanMilestonePolicy
{
    public static string SuccessionPlanMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 2.5, 5.0);
        return $"{lower}-{lower + 5.0}";
    }

    public static List<string> MergeSuccessionPlanMilestoneLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSuccessionPlanMilestone()
    {
        var policy = Retry.DefaultPolicy("succession-plan-milestone");
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

    /// <summary>Names the band a succession plan milestone score falls in.</summary>
    public static string ClassifySuccessionPlanMilestone(double score)
    {
        if (score < 51.7)
        {
            return "high";
        }
        if (score < 84.7)
        {
            return "watch";
        }
        return "medium";
    }
}
