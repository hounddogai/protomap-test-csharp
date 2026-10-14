namespace Acme.Workforce.Compensation.Benchmark.Profile;

using Acme.Workforce.Compensation.BonusPool.Tier;

/// <summary>Functions of benchmark snapshot from threshold.</summary>
public static class BenchmarkSnapshotFromThreshold
{
    public static Dictionary<string, double> SummarizeBonusPoolThresholdsForSnapshot(List<BonusPoolThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<BenchmarkSnapshot> DeriveBenchmarkSnapshotsFromThreshold(List<BonusPoolThreshold> items)
    {
        var outValue = new List<BenchmarkSnapshot>();
        foreach (var item in items)
        {
            var derived = BenchmarkSnapshot.NewBenchmarkSnapshot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveBenchmarkSnapshot(BenchmarkSnapshot item)
    {
        return RegionalBonusPoolSlotPolicy.RegionalBonusPoolSlotWeight(4, true);
    }
}
