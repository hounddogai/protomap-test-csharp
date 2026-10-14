namespace Acme.Workforce.Workplace.Booking.Digest;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Functions of default booking claim from reservation.</summary>
public static class DefaultBookingClaimFromReservation
{
    public static Dictionary<string, double> SummarizeOccupancyReservationsForClaim(List<OccupancyReservation> items)
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

    public static List<DefaultBookingClaim> DeriveDefaultBookingClaimsFromReservation(List<OccupancyReservation> items)
    {
        var outValue = new List<DefaultBookingClaim>();
        foreach (var item in items)
        {
            var derived = DefaultBookingClaim.NewDefaultBookingClaim(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeDefaultBookingClaim(DefaultBookingClaim item)
    {
        return GlobalOccupancyRecordPolicy.NormalizeGlobalOccupancyRecordCode(item.Code);
    }
}
