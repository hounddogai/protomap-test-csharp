namespace Acme.Workforce.Compensation.Benchmark.Baseline;

using Acme.Workforce.Compensation.EquityGrant.Plan;

/// <summary>Functions of regional benchmark forecast from preference.</summary>
public static class RegionalBenchmarkForecastFromPreference
{
    public static Dictionary<string, double> SummarizeBatchedEquityGrantPreferencesForForecast(List<BatchedEquityGrantPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<RegionalBenchmarkForecast> DeriveRegionalBenchmarkForecastsFromPreference(List<BatchedEquityGrantPreference> items)
    {
        var outValue = new List<RegionalBenchmarkForecast>();
        foreach (var item in items)
        {
            var derived = RegionalBenchmarkForecast.NewRegionalBenchmarkForecast(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRegionalBenchmarkForecast(RegionalBenchmarkForecast item)
    {
        return BatchedEquityGrantPreferencePolicy.NormalizeBatchedEquityGrantPreferenceCode(item.Code);
    }
}
