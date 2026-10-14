namespace Acme.Workforce.Compensation.Benchmark.Proposal;

using Acme.Workforce.Compensation.BonusPool.PrimaryPoolSchedule;

/// <summary>Functions of tiered benchmark summary from metric.</summary>
public static class TieredBenchmarkSummaryFromMetric
{
    public static Dictionary<string, double> SummarizeBonusPoolMetricsForSummary(List<BonusPoolMetric> items)
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

    public static List<TieredBenchmarkSummary> DeriveTieredBenchmarkSummariesFromMetric(List<BonusPoolMetric> items)
    {
        var outValue = new List<TieredBenchmarkSummary>();
        foreach (var item in items)
        {
            var derived = TieredBenchmarkSummary.NewTieredBenchmarkSummary(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankTieredBenchmarkSummary(TieredBenchmarkSummary item)
    {
        return BonusPoolForecastPolicy.BonusPoolForecastBucket(item.MeasuredValue());
    }
}
