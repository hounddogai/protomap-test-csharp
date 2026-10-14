namespace Acme.Workforce.Compensation.PayBand.Threshold;

using Acme.Workforce.Compensation.Adjustment.Rule;

/// <summary>Functions of pay band metric from schedule.</summary>
public static class PayBandMetricFromSchedule
{
    public static Dictionary<string, double> SummarizeAdjustmentSchedulesForMetric(List<AdjustmentSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<PayBandMetric> DerivePayBandMetricsFromSchedule(List<AdjustmentSchedule> items)
    {
        var outValue = new List<PayBandMetric>();
        foreach (var item in items)
        {
            var derived = PayBandMetric.NewPayBandMetric(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocatePayBandMetric(PayBandMetric item)
    {
        return AdjustmentRulePolicy.ClassifyAdjustmentRule(item.ProjectedVolume());
    }
}
