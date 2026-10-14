namespace Acme.Workforce.Talent.Pool.Threshold;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of strict pool plan policy.</summary>
public static class StrictPoolPlanPolicy
{
    public static List<string> MergeStrictPoolPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictPoolPlan()
    {
        var policy = Retry.DefaultPolicy("strict-pool-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a strict pool plan score falls in.</summary>
    public static string ClassifyStrictPoolPlan(double score)
    {
        if (score < 38.5)
        {
            return "medium";
        }
        if (score < 45.3)
        {
            return "critical";
        }
        return "high";
    }
}
