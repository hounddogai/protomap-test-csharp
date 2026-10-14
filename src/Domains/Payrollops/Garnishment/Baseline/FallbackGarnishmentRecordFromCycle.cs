namespace Acme.Workforce.Payrollops.Garnishment.Baseline;

using Acme.Workforce.Payrollops.PayRun.Cycle;

/// <summary>Functions of fallback garnishment record from cycle.</summary>
public static class FallbackGarnishmentRecordFromCycle
{
    public static Dictionary<string, double> SummarizeScheduledPayRunCyclesForRecord(List<ScheduledPayRunCycle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<FallbackGarnishmentRecord> DeriveFallbackGarnishmentRecordsFromCycle(List<ScheduledPayRunCycle> items)
    {
        var outValue = new List<FallbackGarnishmentRecord>();
        foreach (var item in items)
        {
            var derived = FallbackGarnishmentRecord.NewFallbackGarnishmentRecord(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeFallbackGarnishmentRecord(FallbackGarnishmentRecord item)
    {
        return LegacyPayRunAssignmentPolicy.LegacyPayRunAssignmentBucket(item.WeightedValue());
    }
}
