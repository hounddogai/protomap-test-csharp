namespace Acme.Workforce.Workplace.Booking.Review;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Functions of booking digest from baseline.</summary>
public static class BookingDigestFromBaseline
{
    public static Dictionary<string, double> SummarizeScheduledBookingBaselinesForDigest(List<ScheduledBookingBaseline> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<BookingDigest> DeriveBookingDigestsFromBaseline(List<ScheduledBookingBaseline> items)
    {
        var outValue = new List<BookingDigest>();
        foreach (var item in items)
        {
            var derived = BookingDigest.NewBookingDigest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateBookingDigest(BookingDigest item)
    {
        return SeasonalBookingSegmentPolicy.SeasonalBookingSegmentWeight(2, true);
    }
}
