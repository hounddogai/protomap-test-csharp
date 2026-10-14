namespace Acme.Workforce.Talent.SuccessionPlan.Window;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of rolling succession plan quota policy.</summary>
public static class RollingSuccessionPlanQuotaPolicy
{
    /// <summary>Names the band a rolling succession plan quota score falls in.</summary>
    public static string ClassifyRollingSuccessionPlanQuota(double score)
    {
        if (score < 13.5)
        {
            return "low";
        }
        if (score < 22.3)
        {
            return "high";
        }
        if (score < 30.1)
        {
            return "watch";
        }
        if (score < 80.0)
        {
            return "critical";
        }
        return "medium";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRollingSuccessionPlanQuota()
    {
        var policy = Retry.DefaultPolicy("rolling-succession-plan-quota");
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

    /// <summary>Counts how many amounts fit in a budget of 48373.0, in order.</summary>
    public static int RollingSuccessionPlanQuotaBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 48373.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
