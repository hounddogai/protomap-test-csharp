namespace Acme.Workforce.Contractors.Rate.Notice;

using Acme.Workforce.Contractors.Timesheet.Batch;

/// <summary>Functions of weighted rate notice from batch.</summary>
public static class WeightedRateNoticeFromBatch
{
    public static Dictionary<string, double> SummarizeGlobalTimesheetBatchesForNotice(List<GlobalTimesheetBatch> items)
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

    public static List<WeightedRateNotice> DeriveWeightedRateNoticesFromBatch(List<GlobalTimesheetBatch> items)
    {
        var outValue = new List<WeightedRateNotice>();
        foreach (var item in items)
        {
            var derived = WeightedRateNotice.NewWeightedRateNotice(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleWeightedRateNotice(WeightedRateNotice item)
    {
        return GlobalTimesheetBatchPolicy.NormalizeGlobalTimesheetBatchCode(item.Code);
    }
}
