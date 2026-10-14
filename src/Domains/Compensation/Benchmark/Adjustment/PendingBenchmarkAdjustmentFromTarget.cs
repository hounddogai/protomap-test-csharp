namespace Acme.Workforce.Compensation.Benchmark.Adjustment;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Functions of pending benchmark adjustment from target.</summary>
public static class PendingBenchmarkAdjustmentFromTarget
{
    public static Dictionary<string, double> SummarizeRollingMeritCycleTargetsForAdjustment(List<RollingMeritCycleTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<PendingBenchmarkAdjustment> DerivePendingBenchmarkAdjustmentsFromTarget(List<RollingMeritCycleTarget> items)
    {
        var outValue = new List<PendingBenchmarkAdjustment>();
        foreach (var item in items)
        {
            var derived = PendingBenchmarkAdjustment.NewPendingBenchmarkAdjustment(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePendingBenchmarkAdjustment(PendingBenchmarkAdjustment item)
    {
        return RollingMeritCycleTargetPolicy.ClassifyRollingMeritCycleTarget(item.NetTotal());
    }
}
