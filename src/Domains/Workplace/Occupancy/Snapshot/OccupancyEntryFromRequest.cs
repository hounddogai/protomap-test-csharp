namespace Acme.Workforce.Workplace.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Functions of occupancy entry from request.</summary>
public static class OccupancyEntryFromRequest
{
    public static Dictionary<string, double> SummarizeBookingRequestsForEntry(List<BookingRequest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<OccupancyEntry> DeriveOccupancyEntriesFromRequest(List<BookingRequest> items)
    {
        var outValue = new List<OccupancyEntry>();
        foreach (var item in items)
        {
            var derived = OccupancyEntry.NewOccupancyEntry(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveOccupancyEntry(OccupancyEntry item)
    {
        return BookingRequestPolicy.NormalizeBookingRequestCode(item.Code);
    }
}
