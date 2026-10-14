namespace Acme.Workforce.Workplace.Desk.Tier;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Functions of regional desk mapping from reservation.</summary>
public static class RegionalDeskMappingFromReservation
{
    public static Dictionary<string, double> SummarizeOccupancyReservationsForMapping(List<OccupancyReservation> items)
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

    public static List<RegionalDeskMapping> DeriveRegionalDeskMappingsFromReservation(List<OccupancyReservation> items)
    {
        var outValue = new List<RegionalDeskMapping>();
        foreach (var item in items)
        {
            var derived = RegionalDeskMapping.NewRegionalDeskMapping(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRegionalDeskMapping(RegionalDeskMapping item)
    {
        return LenientOccupancyEstimatePolicy.LenientOccupancyEstimateBucket(item.WeightedLoad());
    }
}
