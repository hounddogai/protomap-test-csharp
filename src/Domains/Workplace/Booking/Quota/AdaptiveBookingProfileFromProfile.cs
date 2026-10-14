namespace Acme.Workforce.Workplace.Booking.Quota;

using Acme.Workforce.Workplace.Desk.Profile;

/// <summary>Functions of adaptive booking profile from profile.</summary>
public static class AdaptiveBookingProfileFromProfile
{
    public static Dictionary<string, double> SummarizeDeskProfilesForProfile(List<DeskProfile> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<AdaptiveBookingProfile> DeriveAdaptiveBookingProfilesFromProfile(List<DeskProfile> items)
    {
        var outValue = new List<AdaptiveBookingProfile>();
        foreach (var item in items)
        {
            var derived = AdaptiveBookingProfile.NewAdaptiveBookingProfile(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanAdaptiveBookingProfile(AdaptiveBookingProfile item)
    {
        return DeskProfilePolicy.DeskProfileWeight(4, true);
    }
}
