namespace Acme.Workforce.Compensation.MeritCycle.Batch;

using Acme.Workforce.Compensation.Benchmark.Baseline;

/// <summary>Functions of merit cycle bundle from rule.</summary>
public static class MeritCycleBundleFromRule
{
    public static Dictionary<string, double> SummarizeFallbackBenchmarkRulesForBundle(List<FallbackBenchmarkRule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<MeritCycleBundle> DeriveMeritCycleBundlesFromRule(List<FallbackBenchmarkRule> items)
    {
        var outValue = new List<MeritCycleBundle>();
        foreach (var item in items)
        {
            var derived = MeritCycleBundle.NewMeritCycleBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeMeritCycleBundle(MeritCycleBundle item)
    {
        return RegionalBenchmarkForecastPolicy.RegionalBenchmarkForecastBucket(item.AdjustedLoad());
    }
}
