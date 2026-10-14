namespace Acme.Workforce.Payrollops.Deduction.Agreement;

using Acme.Workforce.Payrollops.RetroPay.Window;

/// <summary>Functions of deduction metric from estimate.</summary>
public static class DeductionMetricFromEstimate
{
    public static Dictionary<string, double> SummarizeRetroPayEstimatesForMetric(List<RetroPayEstimate> items)
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

    public static List<DeductionMetric> DeriveDeductionMetricsFromEstimate(List<RetroPayEstimate> items)
    {
        var outValue = new List<DeductionMetric>();
        foreach (var item in items)
        {
            var derived = DeductionMetric.NewDeductionMetric(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MatchDeductionMetric(DeductionMetric item)
    {
        return RetroPayWindowPolicy.RetroPayWindowWeight(3, true);
    }
}
