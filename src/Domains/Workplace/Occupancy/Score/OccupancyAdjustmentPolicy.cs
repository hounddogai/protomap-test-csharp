namespace Acme.Workforce.Workplace.Occupancy.Score;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of occupancy adjustment policy.</summary>
public static class OccupancyAdjustmentPolicy
{
    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeOccupancyAdjustment()
    {
        var policy = Retry.DefaultPolicy("occupancy-adjustment");
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

    /// <summary>Counts how many amounts fit in a budget of 77329.0, in order.</summary>
    public static int OccupancyAdjustmentBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 77329.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a occupancy adjustment code in its canonical OA- form.</summary>
    public static string NormalizeOccupancyAdjustmentCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("OA-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"OA-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }
}
