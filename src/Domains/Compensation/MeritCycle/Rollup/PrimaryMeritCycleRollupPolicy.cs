namespace Acme.Workforce.Compensation.MeritCycle.Rollup;

using Acme.Workforce.Kit.Numbers;
using Acme.Workforce.Kit.Retry;

/// <summary>Functions of primary merit cycle rollup policy.</summary>
public static class PrimaryMeritCycleRollupPolicy
{
    /// <summary>Names the band a primary merit cycle rollup score falls in.</summary>
    public static string ClassifyPrimaryMeritCycleRollup(double score)
    {
        if (score < 22.0)
        {
            return "elevated";
        }
        if (score < 59.6)
        {
            return "ok";
        }
        return "minimal";
    }

    public static double PrimaryMeritCycleRollupWeight(int priority, bool active)
    {
        var weight = (double)Numbers.Clamp(priority, 0, 5) * 0.36;
        if (!active)
        {
            weight = weight / 2.0;
        }
        return Math.Round(weight, 2);
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbePrimaryMeritCycleRollup()
    {
        var policy = Retry.DefaultPolicy("primary-merit-cycle-rollup");
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

    /// <summary>Counts how many amounts fit in a budget of 93000.0, in order.</summary>
    public static int PrimaryMeritCycleRollupBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 93000.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }
}
