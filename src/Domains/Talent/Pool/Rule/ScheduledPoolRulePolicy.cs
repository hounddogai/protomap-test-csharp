namespace Acme.Workforce.Talent.Pool.Rule;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of scheduled pool rule policy.</summary>
public static class ScheduledPoolRulePolicy
{
    public static double ScheduledPoolRuleWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 10) * 2.63;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    public static string ScheduledPoolRuleBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 12.5, 25.0);
        return $"{lower}-{lower + 25.0}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeScheduledPoolRule()
    {
        var policy = Retry.DefaultPolicy("scheduled-pool-rule");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 1);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }

    /// <summary>Names the band a scheduled pool rule score falls in.</summary>
    public static string ClassifyScheduledPoolRule(double score)
    {
        if (score < 2.2)
        {
            return "watch";
        }
        if (score < 8.5)
        {
            return "high";
        }
        if (score < 35.2)
        {
            return "low";
        }
        return "elevated";
    }
}
