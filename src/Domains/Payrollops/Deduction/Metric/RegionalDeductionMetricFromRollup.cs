namespace Acme.Workforce.Payrollops.Deduction.Metric;

using Acme.Workforce.Payrollops.RetroPay.Quota;

/// <summary>Functions of regional deduction metric from rollup.</summary>
public static class RegionalDeductionMetricFromRollup
{
    public static Dictionary<string, double> SummarizeLenientRetroPayRollupsForMetric(List<LenientRetroPayRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<RegionalDeductionMetric> DeriveRegionalDeductionMetricsFromRollup(List<LenientRetroPayRollup> items)
    {
        var outValue = new List<RegionalDeductionMetric>();
        foreach (var item in items)
        {
            var derived = RegionalDeductionMetric.NewRegionalDeductionMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleRegionalDeductionMetric(RegionalDeductionMetric item)
    {
        return RetroPayEntryPolicy.RetroPayEntryWeight(7, true);
    }
}
