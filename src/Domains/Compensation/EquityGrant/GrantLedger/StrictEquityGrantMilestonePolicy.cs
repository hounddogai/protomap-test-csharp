namespace Acme.Workforce.Compensation.EquityGrant.GrantLedger;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of strict equity grant milestone policy.</summary>
public static class StrictEquityGrantMilestonePolicy
{
    public static string StrictEquityGrantMilestoneBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 5.0, 10.0);
        return $"{lower}-{lower + 10.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeStrictEquityGrantMilestone()
    {
        var policy = Retry.DefaultPolicy("strict-equity-grant-milestone");
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

    /// <summary>Names the band a strict equity grant milestone score falls in.</summary>
    public static string ClassifyStrictEquityGrantMilestone(double score)
    {
        if (score < 16.0)
        {
            return "critical";
        }
        if (score < 57.8)
        {
            return "medium";
        }
        if (score < 65.8)
        {
            return "watch";
        }
        return "ok";
    }

    public static double StrictEquityGrantMilestoneWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 9) * 0.66;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }
}
