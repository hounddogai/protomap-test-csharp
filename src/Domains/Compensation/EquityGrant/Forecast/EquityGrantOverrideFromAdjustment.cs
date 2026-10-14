namespace Acme.Workforce.Compensation.EquityGrant.Forecast;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Functions of equity grant override from adjustment.</summary>
public static class EquityGrantOverrideFromAdjustment
{
    public static Dictionary<string, double> SummarizeBenchmarkAdjustmentsForOverride(List<BenchmarkAdjustment> items)
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

    public static List<EquityGrantOverride> DeriveEquityGrantOverridesFromAdjustment(List<BenchmarkAdjustment> items)
    {
        var outValue = new List<EquityGrantOverride>();
        foreach (var item in items)
        {
            var derived = EquityGrantOverride.NewEquityGrantOverride(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignEquityGrantOverride(EquityGrantOverride item)
    {
        return BenchmarkMappingPolicy.BenchmarkMappingBucket(item.AdjustedExposure());
    }
}
