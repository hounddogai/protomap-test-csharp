namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Compensation.Adjustment.Forecast;

/// <summary>Functions of adaptive benchmark metric from checkpoint.</summary>
public static class AdaptiveBenchmarkMetricFromCheckpoint
{
    public static Dictionary<string, double> SummarizeFallbackAdjustmentCheckpointsForMetric(List<FallbackAdjustmentCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<AdaptiveBenchmarkMetric> DeriveAdaptiveBenchmarkMetricsFromCheckpoint(List<FallbackAdjustmentCheckpoint> items)
    {
        var outValue = new List<AdaptiveBenchmarkMetric>();
        foreach (var item in items)
        {
            var derived = AdaptiveBenchmarkMetric.NewAdaptiveBenchmarkMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeAdaptiveBenchmarkMetric(AdaptiveBenchmarkMetric item)
    {
        return AdjustmentForecastPolicy.NormalizeAdjustmentForecastCode(item.Code);
    }
}
