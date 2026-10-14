namespace Acme.Workforce.Payrollops.Deduction.Baseline;

using Acme.Workforce.Payrollops.PayRun.Review;

/// <summary>Functions of deduction entry from entry.</summary>
public static class DeductionEntryFromEntry
{
    public static Dictionary<string, double> SummarizePayRunEntriesForEntry(List<PayRunEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<DeductionEntry> DeriveDeductionEntriesFromEntry(List<PayRunEntry> items)
    {
        var outValue = new List<DeductionEntry>();
        foreach (var item in items)
        {
            var derived = DeductionEntry.NewDeductionEntry(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceDeductionEntry(DeductionEntry item)
    {
        return PayRunAssignmentPolicy.ClassifyPayRunAssignment(item.MeasuredValue());
    }
}
