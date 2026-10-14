namespace Acme.Workforce.Compensation.MeritCycle.Budget;

using Acme.Workforce.Compensation.Benchmark.Adjustment;

/// <summary>Functions of merit cycle threshold from digest.</summary>
public static class MeritCycleThresholdFromDigest
{
    public static Dictionary<string, double> SummarizeBenchmarkDigestsForThreshold(List<BenchmarkDigest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<MeritCycleThreshold> DeriveMeritCycleThresholdsFromDigest(List<BenchmarkDigest> items)
    {
        var outValue = new List<MeritCycleThreshold>();
        foreach (var item in items)
        {
            var derived = MeritCycleThreshold.NewMeritCycleThreshold(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveMeritCycleThreshold(MeritCycleThreshold item)
    {
        return RollingBenchmarkSummaryPolicy.ClassifyRollingBenchmarkSummary(item.EffectiveVolume());
    }
}
