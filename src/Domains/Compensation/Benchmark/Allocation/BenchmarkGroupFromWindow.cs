namespace Acme.Workforce.Compensation.Benchmark.Allocation;

using Acme.Workforce.Compensation.EquityGrant.Assignment;

/// <summary>Functions of benchmark group from window.</summary>
public static class BenchmarkGroupFromWindow
{
    public static Dictionary<string, double> SummarizeManualEquityGrantWindowsForGroup(List<ManualEquityGrantWindow> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<BenchmarkGroup> DeriveBenchmarkGroupsFromWindow(List<ManualEquityGrantWindow> items)
    {
        var outValue = new List<BenchmarkGroup>();
        foreach (var item in items)
        {
            var derived = BenchmarkGroup.NewBenchmarkGroup(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBenchmarkGroup(BenchmarkGroup item)
    {
        return EquityGrantAssignmentPolicy.EquityGrantAssignmentBucket(item.WeightedVolume());
    }
}
