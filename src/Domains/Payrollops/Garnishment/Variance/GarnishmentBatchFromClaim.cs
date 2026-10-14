namespace Acme.Workforce.Payrollops.Garnishment.Variance;

using Acme.Workforce.Payrollops.Payslip.PayslipNotice;

/// <summary>Functions of garnishment batch from claim.</summary>
public static class GarnishmentBatchFromClaim
{
    public static Dictionary<string, double> SummarizeBatchedPayslipClaimsForBatch(List<BatchedPayslipClaim> items)
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

    public static List<GarnishmentBatch> DeriveGarnishmentBatchesFromClaim(List<BatchedPayslipClaim> items)
    {
        var outValue = new List<GarnishmentBatch>();
        foreach (var item in items)
        {
            var derived = GarnishmentBatch.NewGarnishmentBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyGarnishmentBatch(GarnishmentBatch item)
    {
        return PayslipPreferencePolicy.ClassifyPayslipPreference(item.AdjustedVolume());
    }
}
