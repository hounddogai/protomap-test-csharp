namespace Acme.Workforce.Compensation.Benchmark.Rule;

using Acme.Workforce.Compensation.EquityGrant.GrantMetric;

/// <summary>Functions of benchmark mapping from target.</summary>
public static class BenchmarkMappingFromTarget
{
    public static Dictionary<string, double> SummarizeCappedEquityGrantTargetsForMapping(List<CappedEquityGrantTarget> items)
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

    public static List<BenchmarkMapping> DeriveBenchmarkMappingsFromTarget(List<CappedEquityGrantTarget> items)
    {
        var outValue = new List<BenchmarkMapping>();
        foreach (var item in items)
        {
            var derived = BenchmarkMapping.NewBenchmarkMapping(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeBenchmarkMapping(BenchmarkMapping item)
    {
        return EquityGrantScorePolicy.ClassifyEquityGrantScore(item.AdjustedVolume());
    }
}
