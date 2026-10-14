namespace Acme.Workforce.Workplace.Occupancy.Estimate;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of tiered occupancy allocation policy.</summary>
public static class TieredOccupancyAllocationPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 14846.0, in order.</summary>
    public static int TieredOccupancyAllocationBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 14846.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a tiered occupancy allocation code in its canonical TOA- form.</summary>
    public static string NormalizeTieredOccupancyAllocationCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("TOA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"TOA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeTieredOccupancyAllocation()
    {
        var policy = Retry.DefaultPolicy("tiered-occupancy-allocation");
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
