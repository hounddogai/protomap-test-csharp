namespace Acme.Workforce.Payrollops.PayRun.Allocation;

using Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Functions of pay run allocation from record.</summary>
public static class PayRunAllocationFromRecord
{
    public static Dictionary<string, double> SummarizePayRunRecordsForAllocation(List<PayRunRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<PayRunAllocation> DerivePayRunAllocationsFromRecord(List<PayRunRecord> items)
    {
        var outValue = new List<PayRunAllocation>();
        foreach (var item in items)
        {
            var derived = PayRunAllocation.NewPayRunAllocation(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankPayRunAllocation(PayRunAllocation item)
    {
        return PayRunSchedulePolicy.NormalizePayRunScheduleCode(item.Code);
    }
}
