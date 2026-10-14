namespace Acme.Workforce.Compensation.PayBand.Batch;

using Acme.Workforce.Compensation.EquityGrant.Batch;

/// <summary>Functions of pay band window from metric.</summary>
public static class PayBandWindowFromMetric
{
    public static Dictionary<string, double> SummarizeStrictEquityGrantMetricsForWindow(List<StrictEquityGrantMetric> items)
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

    public static List<PayBandWindow> DerivePayBandWindowsFromMetric(List<StrictEquityGrantMetric> items)
    {
        var outValue = new List<PayBandWindow>();
        foreach (var item in items)
        {
            var derived = PayBandWindow.NewPayBandWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcilePayBandWindow(PayBandWindow item)
    {
        return EquityGrantBatchPolicy.EquityGrantBatchBucket(item.EffectiveTotal());
    }
}
