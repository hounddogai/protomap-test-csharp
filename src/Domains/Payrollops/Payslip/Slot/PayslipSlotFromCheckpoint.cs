namespace Acme.Workforce.Payrollops.Payslip.Slot;

using Acme.Workforce.Payrollops.PayRun.Checkpoint;

/// <summary>Functions of payslip slot from checkpoint.</summary>
public static class PayslipSlotFromCheckpoint
{
    public static Dictionary<string, double> SummarizePayRunCheckpointsForSlot(List<PayRunCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<PayslipSlot> DerivePayslipSlotsFromCheckpoint(List<PayRunCheckpoint> items)
    {
        var outValue = new List<PayslipSlot>();
        foreach (var item in items)
        {
            var derived = PayslipSlot.NewPayslipSlot(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergePayslipSlot(PayslipSlot item)
    {
        return PayRunCheckpointPolicy.NormalizePayRunCheckpointCode(item.Code);
    }
}
