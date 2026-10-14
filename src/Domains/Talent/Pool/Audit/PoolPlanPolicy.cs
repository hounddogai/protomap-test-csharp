namespace Acme.Workforce.Talent.Pool.Audit;

using Acme.Workforce.Kit.Collections;
using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;
using Acme.Workforce.Kit.Text;

/// <summary>Functions of pool plan policy.</summary>
public static class PoolPlanPolicy
{
    public static double PoolPlanWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 2.85;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Names the band a pool plan score falls in.</summary>
    public static string ClassifyPoolPlan(double score)
    {
        if (score < 7.9)
        {
            return "high";
        }
        if (score < 59.6)
        {
            return "low";
        }
        if (score < 69.2)
        {
            return "watch";
        }
        return "critical";
    }

    public static List<string> MergePoolPlanLabels(List<string> left, List<string> right)
    {
        var merged = new List<string>(left);
        foreach (var tag in right)
        {
            merged.Add(Text.Normalize(tag));
        }
        return Collections.UniqueStrings(merged);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePoolPlan()
    {
        var policy = Retry.DefaultPolicy("pool-plan");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 3);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
