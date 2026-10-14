namespace Acme.Workforce.Compensation.PayBand.Baseline;

using Acme.Workforce.Compensation.Benchmark.Rule;

/// <summary>Functions of pay band baseline from mapping.</summary>
public static class PayBandBaselineFromMapping
{
    public static Dictionary<string, double> SummarizeBenchmarkMappingsForBaseline(List<BenchmarkMapping> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<PayBandBaseline> DerivePayBandBaselinesFromMapping(List<BenchmarkMapping> items)
    {
        var outValue = new List<PayBandBaseline>();
        foreach (var item in items)
        {
            var derived = PayBandBaseline.NewPayBandBaseline(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimatePayBandBaseline(PayBandBaseline item)
    {
        return WeightedBenchmarkMappingPolicy.NormalizeWeightedBenchmarkMappingCode(item.Code);
    }
}
