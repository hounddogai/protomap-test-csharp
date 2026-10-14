namespace Acme.Workforce.Payrollops.PayRun.Request;

using Acme.Workforce.Payrollops.TaxForm.Threshold;

/// <summary>Functions of pay run quota from segment.</summary>
public static class PayRunQuotaFromSegment
{
    public static Dictionary<string, double> SummarizeTaxFormSegmentsForQuota(List<TaxFormSegment> items)
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

    public static List<PayRunQuota> DerivePayRunQuotasFromSegment(List<TaxFormSegment> items)
    {
        var outValue = new List<PayRunQuota>();
        foreach (var item in items)
        {
            var derived = PayRunQuota.NewPayRunQuota(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalancePayRunQuota(PayRunQuota item)
    {
        return TaxFormEstimatePolicy.ClassifyTaxFormEstimate(item.NetVolume());
    }
}
