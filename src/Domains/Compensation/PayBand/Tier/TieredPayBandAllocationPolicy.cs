namespace Acme.Workforce.Compensation.PayBand.Tier;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered pay band allocation policy.</summary>
public static class TieredPayBandAllocationPolicy
{
    /// <summary>Names the band a tiered pay band allocation score falls in.</summary>
    public static string ClassifyTieredPayBandAllocation(double score)
    {
        if (score < 36.5)
        {
            return "minimal";
        }
        if (score < 77.1)
        {
            return "ok";
        }
        return "elevated";
    }

    /// <summary>Counts how many amounts fit in a budget of 58955.0, in order.</summary>
    public static int TieredPayBandAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 58955.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredPayBandAllocation()
    {
        var policy = Retry.DefaultPolicy("tiered-pay-band-allocation");
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
}
