namespace Acme.Workforce.Workplace.FloorPlan.Metric;

using Acme.Workforce.Kit.Retry;

/// <summary>Functions of manual floor plan metric policy.</summary>
public static class ManualFloorPlanMetricPolicy
{
    /// <summary>Puts a manual floor plan metric code in its canonical MFPM- form.</summary>
    public static string NormalizeManualFloorPlanMetricCode(string code)
    {
        var clean = code.Trim().ToUpperInvariant();
        if (clean == "")
        {
            return "";
        }
        if (clean.StartsWith("MFPM-", StringComparison.Ordinal))
        {
            return clean;
        }
        return $"MFPM-{clean.Replace(" ", "-", StringComparison.Ordinal)}";
    }

    /// <summary>Counts how many amounts fit in a budget of 43258.0, in order.</summary>
    public static int ManualFloorPlanMetricBudgetFit(List<double> amounts)
    {
        var spent = 0.0;
        var accepted = 0;
        foreach (var amount in amounts)
        {
            if (spent + amount > 43258.0)
            {
                break;
            }
            spent += amount;
            accepted += 1;
        }
        return accepted;
    }

    /// <summary>Runs the default retry policy against a probe that succeeds late.</summary>
    public static int ProbeManualFloorPlanMetric()
    {
        var policy = Retry.DefaultPolicy("manual-floor-plan-metric");
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

    public static int ManualFloorPlanMetricSpan(int start, int end)
    {
        if (end < start)
        {
            return 0;
        }
        var span = end - start;
        // No grace period.
        return span / 86400;
    }
}
