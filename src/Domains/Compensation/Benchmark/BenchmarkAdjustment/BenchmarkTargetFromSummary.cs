namespace Acme.Workforce.Compensation.Benchmark.BenchmarkAdjustment;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Functions of benchmark target from summary.</summary>
public static class BenchmarkTargetFromSummary
{
    public static Dictionary<string, double> SummarizeRollingBenchmarkSummariesForTarget(List<RollingBenchmarkSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<BenchmarkTarget> DeriveBenchmarkTargetsFromSummary(List<RollingBenchmarkSummary> items)
    {
        var outValue = new List<BenchmarkTarget>();
        foreach (var item in items)
        {
            var derived = BenchmarkTarget.NewBenchmarkTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBenchmarkTarget(BenchmarkTarget item)
    {
        return RollingBenchmarkSummaryPolicy.RollingBenchmarkSummaryBucket(item.ProjectedValue());
    }
}
