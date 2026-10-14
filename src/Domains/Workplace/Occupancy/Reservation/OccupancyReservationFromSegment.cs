namespace Acme.Workforce.Workplace.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Segment;

/// <summary>Functions of occupancy reservation from segment.</summary>
public static class OccupancyReservationFromSegment
{
    public static Dictionary<string, double> SummarizeOccupancySegmentsForReservation(List<OccupancySegment> items)
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

    public static List<OccupancyReservation> DeriveOccupancyReservationsFromSegment(List<OccupancySegment> items)
    {
        var outValue = new List<OccupancyReservation>();
        foreach (var item in items)
        {
            var derived = OccupancyReservation.NewOccupancyReservation(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateOccupancyReservation(OccupancyReservation item)
    {
        return OccupancyMilestonePolicy.OccupancyMilestoneBucket(item.ProjectedTotal());
    }
}
