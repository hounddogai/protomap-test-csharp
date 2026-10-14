namespace Acme.Workforce.Payrollops.Garnishment.GarnishmentBudget;

using Acme.Workforce.Payrollops.PayRun.RunAudit;

/// <summary>Functions of garnishment slot from batch.</summary>
public static class GarnishmentSlotFromBatch
{
    public static Dictionary<string, double> SummarizeRegionalPayRunBatchesForSlot(List<RegionalPayRunBatch> items)
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

    public static List<GarnishmentSlot> DeriveGarnishmentSlotsFromBatch(List<RegionalPayRunBatch> items)
    {
        var outValue = new List<GarnishmentSlot>();
        foreach (var item in items)
        {
            var derived = GarnishmentSlot.NewGarnishmentSlot(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitGarnishmentSlot(GarnishmentSlot item)
    {
        return CappedPayRunLedgerPolicy.CappedPayRunLedgerBucket(item.WeightedVolume());
    }
}
