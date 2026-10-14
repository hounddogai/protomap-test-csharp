namespace Acme.Workforce.Workplace.Booking.Ledger;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Functions of booking bundle from segment.</summary>
public static class BookingBundleFromSegment
{
    public static Dictionary<string, double> SummarizeBookingSegmentsForBundle(List<BookingSegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<BookingBundle> DeriveBookingBundlesFromSegment(List<BookingSegment> items)
    {
        var outValue = new List<BookingBundle>();
        foreach (var item in items)
        {
            var derived = BookingBundle.NewBookingBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeBookingBundle(BookingBundle item)
    {
        return BookingEntryPolicy.BookingEntryWeight(9, true);
    }
}
