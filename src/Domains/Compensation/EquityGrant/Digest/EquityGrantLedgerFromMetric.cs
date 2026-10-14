namespace Acme.Workforce.Compensation.EquityGrant.Digest;

using Acme.Workforce.Compensation.PayBand.Threshold;

/// <summary>Functions of equity grant ledger from metric.</summary>
public static class EquityGrantLedgerFromMetric
{
    public static Dictionary<string, double> SummarizePayBandMetricsForLedger(List<PayBandMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<EquityGrantLedger> DeriveEquityGrantLedgersFromMetric(List<PayBandMetric> items)
    {
        var outValue = new List<EquityGrantLedger>();
        foreach (var item in items)
        {
            var derived = EquityGrantLedger.NewEquityGrantLedger(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double RankEquityGrantLedger(EquityGrantLedger item)
    {
        return PayBandMetricPolicy.PayBandMetricWeight(7, true);
    }
}
