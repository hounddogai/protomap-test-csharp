namespace Acme.Workforce.Engagement.ActionPlan.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of action plan tier policy.</summary>
public static class ActionPlanTierPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeActionPlanTier()
    {
        var policy = Retry.DefaultPolicy("action-plan-tier");
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

    /// <summary>Names the band a action plan tier score falls in.</summary>
    public static string ClassifyActionPlanTier(double score)
    {
        if (score < 5.3)
        {
            return "critical";
        }
        if (score < 60.3)
        {
            return "elevated";
        }
        return "medium";
    }

    public static string ActionPlanTierBucket(double valueValue)
    {
        var lower = Numbers.RoundTo(valueValue - 50.0, 100.0);
        return $"{lower}-{lower + 100.0}";
    }
}
