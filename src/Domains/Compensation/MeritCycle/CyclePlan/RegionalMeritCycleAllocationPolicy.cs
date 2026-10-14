namespace Acme.Workforce.Compensation.MeritCycle.CyclePlan;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of regional merit cycle allocation policy.</summary>
public static class RegionalMeritCycleAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 72385.0, in order.</summary>
    public static int RegionalMeritCycleAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 72385.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a regional merit cycle allocation code in its canonical RMCA- form.</summary>
    public static string NormalizeRegionalMeritCycleAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("RMCA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"RMCA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    public static int RegionalMeritCycleAllocationSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        if (span < 3600)
        {
            return 0;
        }
        return span / 3600;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeRegionalMeritCycleAllocation()
    {
        var policy = Retry.DefaultPolicy("regional-merit-cycle-allocation");
        var calls = 0;
        int used;
        try
        {
            used = policy.Run((int attempt) => attempt >= 4);
        }
        catch (InvalidOperationException)
        {
            return -1;
        }
        calls += used;
        return calls;
    }
}
