namespace Acme.Workforce.Payrollops.PayRun.Metric;

using Acme.Workforce.Payrollops.Payslip.Slot;

/// <summary>Functions of pay run plan from slot.</summary>
public static class PayRunPlanFromSlot
{
    public static Dictionary<string, double> SummarizePayslipSlotsForPlan(List<PayslipSlot> items)
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

    public static List<PayRunPlan> DerivePayRunPlansFromSlot(List<PayslipSlot> items)
    {
        var outValue = new List<PayRunPlan>();
        foreach (var item in items)
        {
            var derived = PayRunPlan.NewPayRunPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double MergePayRunPlan(PayRunPlan item)
    {
        return PayslipRequestPolicy.PayslipRequestWeight(7, true);
    }
}
