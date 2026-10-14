namespace Acme.Workforce.Payrollops.PayRun.Ledger;

using Acme.Workforce.Payrollops.PayRun.Metric;

/// <summary>Functions of pay run ledger from metric.</summary>
public static class PayRunLedgerFromMetric
{
    public static Dictionary<string, double> SummarizeBatchedPayRunMetricsForLedger(List<BatchedPayRunMetric> items)
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

    public static List<PayRunLedger> DerivePayRunLedgersFromMetric(List<BatchedPayRunMetric> items)
    {
        var outValue = new List<PayRunLedger>();
        foreach (var item in items)
        {
            var derived = PayRunLedger.NewPayRunLedger(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePayRunLedger(PayRunLedger item)
    {
        return PayRunMetricPolicy.PayRunMetricBucket(item.MeasuredValue());
    }
}
