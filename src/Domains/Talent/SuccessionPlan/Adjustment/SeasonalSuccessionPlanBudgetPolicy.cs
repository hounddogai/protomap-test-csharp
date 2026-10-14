namespace Acme.Workforce.Talent.SuccessionPlan.Adjustment;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of seasonal succession plan budget policy.</summary>
public static class SeasonalSuccessionPlanBudgetPolicy
{
    public static int SeasonalSuccessionPlanBudgetSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 900)
        {
            return 0;
        }
        return span / 60;
    }

    /// <summary>Names the band a seasonal succession plan budget score falls in.</summary>
    public static string ClassifySeasonalSuccessionPlanBudget(double score)
    {
        if (score < 32.6)
        {
            return "ok";
        }
        if (score < 68.7)
        {
            return "critical";
        }
        if (score < 88.5)
        {
            return "watch";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 31261.0, in order.</summary>
    public static int SeasonalSuccessionPlanBudgetBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 31261.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeSeasonalSuccessionPlanBudget()
    {
        var policy = Retry.DefaultPolicy("seasonal-succession-plan-budget");
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
}
