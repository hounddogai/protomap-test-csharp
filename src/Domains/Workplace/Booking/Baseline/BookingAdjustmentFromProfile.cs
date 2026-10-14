namespace Acme.Workforce.Workplace.Booking.Baseline;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Functions of booking adjustment from profile.</summary>
public static class BookingAdjustmentFromProfile
{
    public static Dictionary<string, double> SummarizeBookingProfilesForAdjustment(List<BookingProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<BookingAdjustment> DeriveBookingAdjustmentsFromProfile(List<BookingProfile> items)
    {
        var outValue = new List<BookingAdjustment>();
        foreach (var item in items)
        {
            var derived = BookingAdjustment.NewBookingAdjustment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveBookingAdjustment(BookingAdjustment item)
    {
        return BookingPreferencePolicy.BookingPreferenceWeight(2, true);
    }
}
