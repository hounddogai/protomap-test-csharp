namespace Acme.Workforce.Payrollops.PayRun.Adjustment;

using Acme.Workforce.Payrollops.RetroPay.PayAssignment;

/// <summary>Functions of pay run adjustment from summary.</summary>
public static class PayRunAdjustmentFromSummary
{
    public static Dictionary<string, double> SummarizeLegacyRetroPaySummariesForAdjustment(List<LegacyRetroPaySummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedValue();
        }
        return totals;
    }

    public static List<PayRunAdjustment> DerivePayRunAdjustmentsFromSummary(List<LegacyRetroPaySummary> items)
    {
        var outValue = new List<PayRunAdjustment>();
        foreach (var item in items)
        {
            var derived = PayRunAdjustment.NewPayRunAdjustment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyPayRunAdjustment(PayRunAdjustment item)
    {
        return RetroPayVariancePolicy.RetroPayVarianceBucket(item.MeasuredValue());
    }
}
