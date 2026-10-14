namespace Acme.Workforce.Workplace.Booking.Target;

using Acme.Workforce.Workplace.Booking.Quota;

/// <summary>Functions of booking target from quota.</summary>
public static class BookingTargetFromQuota
{
    public static Dictionary<string, double> SummarizeBookingQuotasForTarget(List<BookingQuota> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<BookingTarget> DeriveBookingTargetsFromQuota(List<BookingQuota> items)
    {
        var outValue = new List<BookingTarget>();
        foreach (var item in items)
        {
            var derived = BookingTarget.NewBookingTarget(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitBookingTarget(BookingTarget item)
    {
        return BookingQuotaPolicy.BookingQuotaBucket(item.MeasuredValue());
    }
}
