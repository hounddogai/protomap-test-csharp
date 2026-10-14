namespace Acme.Workforce.Workplace.Booking.Entry;

using Acme.Workforce.Workplace.AccessBadge.Estimate;

/// <summary>Functions of tiered booking record from checkpoint.</summary>
public static class TieredBookingRecordFromCheckpoint
{
    public static Dictionary<string, double> SummarizeAdaptiveAccessBadgeCheckpointsForRecord(List<AdaptiveAccessBadgeCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<TieredBookingRecord> DeriveTieredBookingRecordsFromCheckpoint(List<AdaptiveAccessBadgeCheckpoint> items)
    {
        var outValue = new List<TieredBookingRecord>();
        foreach (var item in items)
        {
            var derived = TieredBookingRecord.NewTieredBookingRecord(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SplitTieredBookingRecord(TieredBookingRecord item)
    {
        return GlobalAccessBadgeEstimatePolicy.GlobalAccessBadgeEstimateWeight(4, true);
    }
}
