namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of weighted floor plan entry policy.</summary>
public static class WeightedFloorPlanEntryPolicy
{
    /// <summary>Counts how many amounts fit in a budget of 37296.0, in order.</summary>
    public static int WeightedFloorPlanEntryBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 37296.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Puts a weighted floor plan entry code in its canonical WFPE- form.</summary>
    public static string NormalizeWeightedFloorPlanEntryCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("WFPE-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"WFPE-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeWeightedFloorPlanEntry()
    {
        var policy = Retry.DefaultPolicy("weighted-floor-plan-entry");
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
