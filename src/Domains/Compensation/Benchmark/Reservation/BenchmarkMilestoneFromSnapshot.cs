namespace Acme.Workforce.Compensation.Benchmark.Reservation;

using Acme.Workforce.Compensation.Adjustment.Notice;

/// <summary>Functions of benchmark milestone from snapshot.</summary>
public static class BenchmarkMilestoneFromSnapshot
{
    public static Dictionary<string, double> SummarizeAdjustmentSnapshotsForMilestone(List<AdjustmentSnapshot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<BenchmarkMilestone> DeriveBenchmarkMilestonesFromSnapshot(List<AdjustmentSnapshot> items)
    {
        var outValue = new List<BenchmarkMilestone>();
        foreach (var item in items)
        {
            var derived = BenchmarkMilestone.NewBenchmarkMilestone(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateBenchmarkMilestone(BenchmarkMilestone item)
    {
        return AdjustmentNoticePolicy.ClassifyAdjustmentNotice(item.WeightedLoad());
    }
}
