namespace Acme.Workforce.Payrollops.RetroPay.Summary;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Functions of rolling retro pay summary from slot.</summary>
public static class RollingRetroPaySummaryFromSlot
{
    public static Dictionary<string, double> SummarizePayslipSlotsForSummary(List<PayslipSlot> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<RollingRetroPaySummary> DeriveRollingRetroPaySummariesFromSlot(List<PayslipSlot> items)
    {
        var outValue = new List<RollingRetroPaySummary>();
        foreach (var item in items)
        {
            var derived = RollingRetroPaySummary.NewRollingRetroPaySummary(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateRollingRetroPaySummary(RollingRetroPaySummary item)
    {
        return PayslipSlotPolicy.PayslipSlotWeight(6, true);
    }
}
