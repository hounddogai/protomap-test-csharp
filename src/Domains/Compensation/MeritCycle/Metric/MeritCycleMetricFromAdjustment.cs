namespace Acme.Workforce.Compensation.MeritCycle.Metric;

using Acme.Workforce.Compensation.Benchmark.AdaptiveBenchmarkAdjustment;

/// <summary>Functions of merit cycle metric from adjustment.</summary>
public static class MeritCycleMetricFromAdjustment
{
    public static Dictionary<string, double> SummarizeRollingBenchmarkAdjustmentsForMetric(List<RollingBenchmarkAdjustment> items)
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

    public static List<MeritCycleMetric> DeriveMeritCycleMetricsFromAdjustment(List<RollingBenchmarkAdjustment> items)
    {
        var outValue = new List<MeritCycleMetric>();
        foreach (var item in items)
        {
            var derived = MeritCycleMetric.NewMeritCycleMetric(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyMeritCycleMetric(MeritCycleMetric item)
    {
        return RollingBenchmarkAdjustmentPolicy.RollingBenchmarkAdjustmentBucket(item.ProjectedVolume());
    }
}
