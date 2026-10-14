namespace Acme.Workforce.Payrollops.Deduction.DeductionBaseline;

using Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Functions of global deduction baseline from record.</summary>
public static class GlobalDeductionBaselineFromRecord
{
    public static Dictionary<string, double> SummarizePayRunRecordsForBaseline(List<PayRunRecord> items)
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

    public static List<GlobalDeductionBaseline> DeriveGlobalDeductionBaselinesFromRecord(List<PayRunRecord> items)
    {
        var outValue = new List<GlobalDeductionBaseline>();
        foreach (var item in items)
        {
            var derived = GlobalDeductionBaseline.NewGlobalDeductionBaseline(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeGlobalDeductionBaseline(GlobalDeductionBaseline item)
    {
        return PayRunRecordPolicy.NormalizePayRunRecordCode(item.Code);
    }
}
