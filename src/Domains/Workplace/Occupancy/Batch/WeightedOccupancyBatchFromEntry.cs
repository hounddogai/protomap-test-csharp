namespace Acme.Workforce.Workplace.Occupancy.Batch;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Functions of weighted occupancy batch from entry.</summary>
public static class WeightedOccupancyBatchFromEntry
{
    public static Dictionary<string, double> SummarizeBookingEntriesForBatch(List<BookingEntry> items)
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

    public static List<WeightedOccupancyBatch> DeriveWeightedOccupancyBatchesFromEntry(List<BookingEntry> items)
    {
        var outValue = new List<WeightedOccupancyBatch>();
        foreach (var item in items)
        {
            var derived = WeightedOccupancyBatch.NewWeightedOccupancyBatch(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleWeightedOccupancyBatch(WeightedOccupancyBatch item)
    {
        return BookingLedgerPolicy.NormalizeBookingLedgerCode(item.Code);
    }
}
