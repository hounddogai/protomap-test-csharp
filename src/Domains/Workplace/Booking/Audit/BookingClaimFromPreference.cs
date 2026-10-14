namespace Acme.Workforce.Workplace.Booking.Audit;

using Acme.Workforce.Workplace.Desk.Mapping;

/// <summary>Functions of booking claim from preference.</summary>
public static class BookingClaimFromPreference
{
    public static Dictionary<string, double> SummarizeDeskPreferencesForClaim(List<DeskPreference> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<BookingClaim> DeriveBookingClaimsFromPreference(List<DeskPreference> items)
    {
        var outValue = new List<BookingClaim>();
        foreach (var item in items)
        {
            var derived = BookingClaim.NewBookingClaim(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeBookingClaim(BookingClaim item)
    {
        return DeskPreferencePolicy.DeskPreferenceBucket(item.ProjectedLoad());
    }
}
