namespace Acme.Workforce.Compensation.Adjustment.Request;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered adjustment mapping policy.</summary>
public static class TieredAdjustmentMappingPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 19838.0, in order.</summary>
    public static int TieredAdjustmentMappingBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 19838.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredAdjustmentMapping()
    {
        var policy = Retry.DefaultPolicy("tiered-adjustment-mapping");
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
