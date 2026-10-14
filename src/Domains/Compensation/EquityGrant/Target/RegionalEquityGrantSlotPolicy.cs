namespace Acme.Workforce.Compensation.EquityGrant.Target;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional equity grant slot policy.</summary>
public static class RegionalEquityGrantSlotPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalEquityGrantSlot()
    {
        var policy = Retry.DefaultPolicy("regional-equity-grant-slot");
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

    /// <summary>Counts how many amounts fit in a budget of 68051.0, in order.</summary>
    public static int RegionalEquityGrantSlotBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 68051.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Names the band a regional equity grant slot score falls in.</summary>
    public static string ClassifyRegionalEquityGrantSlot(double score)
    {
        if (score < 67.7)
        {
            return "medium";
        }
        if (score < 92.9)
        {
            return "elevated";
        }
        return "ok";
    }
}
