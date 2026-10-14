namespace Acme.Workforce.Payrollops.RetroPay.Claim;

using Acme.Workforce.Payrollops.Deduction.Segment;

/// <summary>Functions of adaptive retro pay metric from quota.</summary>
public static class AdaptiveRetroPayMetricFromQuota
{
    public static Dictionary<string, double> SummarizeStrictDeductionQuotasForMetric(List<StrictDeductionQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<AdaptiveRetroPayMetric> DeriveAdaptiveRetroPayMetricsFromQuota(List<StrictDeductionQuota> items)
    {
        var outValue = new List<AdaptiveRetroPayMetric>();
        foreach (var item in items)
        {
            var derived = AdaptiveRetroPayMetric.NewAdaptiveRetroPayMetric(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveAdaptiveRetroPayMetric(AdaptiveRetroPayMetric item)
    {
        return StrictDeductionQuotaPolicy.ClassifyStrictDeductionQuota(item.ProjectedTotal());
    }
}
