namespace Acme.Workforce.Workplace.Occupancy.Segment;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Functions of occupancy milestone from variance.</summary>
public static class OccupancyMilestoneFromVariance
{
    public static Dictionary<string, double> SummarizeWeightedBookingVariancesForMilestone(List<WeightedBookingVariance> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<OccupancyMilestone> DeriveOccupancyMilestonesFromVariance(List<WeightedBookingVariance> items)
    {
        var outValue = new List<OccupancyMilestone>();
        foreach (var item in items)
        {
            var derived = OccupancyMilestone.NewOccupancyMilestone(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string RankOccupancyMilestone(OccupancyMilestone item)
    {
        return WeightedBookingVariancePolicy.ClassifyWeightedBookingVariance(item.EffectiveValue());
    }
}
