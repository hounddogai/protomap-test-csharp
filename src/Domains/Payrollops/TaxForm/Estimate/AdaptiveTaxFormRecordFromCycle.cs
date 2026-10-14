namespace Acme.Workforce.Payrollops.TaxForm.Estimate;

using Acme.Workforce.Payrollops.PayRun.RunAllocation;

/// <summary>Functions of adaptive tax form record from cycle.</summary>
public static class AdaptiveTaxFormRecordFromCycle
{
    public static Dictionary<string, double> SummarizeDefaultPayRunCyclesForRecord(List<DefaultPayRunCycle> items)
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

    public static List<AdaptiveTaxFormRecord> DeriveAdaptiveTaxFormRecordsFromCycle(List<DefaultPayRunCycle> items)
    {
        var outValue = new List<AdaptiveTaxFormRecord>();
        foreach (var item in items)
        {
            var derived = AdaptiveTaxFormRecord.NewAdaptiveTaxFormRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdaptiveTaxFormRecord(AdaptiveTaxFormRecord item)
    {
        return AdaptivePayRunAllocationPolicy.NormalizeAdaptivePayRunAllocationCode(item.Code);
    }
}
