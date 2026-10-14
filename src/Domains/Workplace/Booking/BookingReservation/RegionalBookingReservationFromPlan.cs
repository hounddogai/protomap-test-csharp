namespace Acme.Workforce.Workplace.Booking.BookingReservation;

using Acme.Workforce.Workplace.AccessBadge.Plan;

/// <summary>Functions of regional booking reservation from plan.</summary>
public static class RegionalBookingReservationFromPlan
{
    public static Dictionary<string, double> SummarizeAccessBadgePlansForReservation(List<AccessBadgePlan> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<RegionalBookingReservation> DeriveRegionalBookingReservationsFromPlan(List<AccessBadgePlan> items)
    {
        var outValue = new List<RegionalBookingReservation>();
        foreach (var item in items)
        {
            var derived = RegionalBookingReservation.NewRegionalBookingReservation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalanceRegionalBookingReservation(RegionalBookingReservation item)
    {
        return DefaultAccessBadgeOverridePolicy.DefaultAccessBadgeOverrideWeight(9, true);
    }
}
