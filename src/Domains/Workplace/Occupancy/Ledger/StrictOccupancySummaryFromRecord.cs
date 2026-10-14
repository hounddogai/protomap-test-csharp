namespace Acme.Workforce.Workplace.Occupancy.Ledger;

using Acme.Workforce.Workplace.Occupancy.OccupancyAgreement;

/// <summary>Functions of strict occupancy summary from record.</summary>
public static class StrictOccupancySummaryFromRecord
{
    public static Dictionary<string, double> SummarizeLegacyOccupancyRecordsForSummary(List<LegacyOccupancyRecord> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<StrictOccupancySummary> DeriveStrictOccupancySummariesFromRecord(List<LegacyOccupancyRecord> items)
    {
        var outValue = new List<StrictOccupancySummary>();
        foreach (var item in items)
        {
            var derived = StrictOccupancySummary.NewStrictOccupancySummary(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeStrictOccupancySummary(StrictOccupancySummary item)
    {
        return LegacyOccupancyRecordPolicy.LegacyOccupancyRecordBucket(item.NetExposure());
    }
}
