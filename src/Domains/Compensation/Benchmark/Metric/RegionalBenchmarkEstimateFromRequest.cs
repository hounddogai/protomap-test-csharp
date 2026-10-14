namespace Acme.Workforce.Compensation.Benchmark.Metric;

using Acme.Workforce.Compensation.PayBand.Milestone;

/// <summary>Functions of regional benchmark estimate from request.</summary>
public static class RegionalBenchmarkEstimateFromRequest
{
    public static Dictionary<string, double> SummarizeLegacyPayBandRequestsForEstimate(List<LegacyPayBandRequest> items)
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

    public static List<RegionalBenchmarkEstimate> DeriveRegionalBenchmarkEstimatesFromRequest(List<LegacyPayBandRequest> items)
    {
        var outValue = new List<RegionalBenchmarkEstimate>();
        foreach (var item in items)
        {
            var derived = RegionalBenchmarkEstimate.NewRegionalBenchmarkEstimate(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRegionalBenchmarkEstimate(RegionalBenchmarkEstimate item)
    {
        return LegacyPayBandRequestPolicy.LegacyPayBandRequestBucket(item.MeasuredValue());
    }
}
