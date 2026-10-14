namespace Acme.Workforce.Workplace.Occupancy.Plan;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Functions of strict occupancy review from assignment.</summary>
public static class StrictOccupancyReviewFromAssignment
{
    public static Dictionary<string, double> SummarizeTieredBookingAssignmentsForReview(List<TieredBookingAssignment> items)
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

    public static List<StrictOccupancyReview> DeriveStrictOccupancyReviewsFromAssignment(List<TieredBookingAssignment> items)
    {
        var outValue = new List<StrictOccupancyReview>();
        foreach (var item in items)
        {
            var derived = StrictOccupancyReview.NewStrictOccupancyReview(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitStrictOccupancyReview(StrictOccupancyReview item)
    {
        return TieredBookingAssignmentPolicy.ClassifyTieredBookingAssignment(item.NetTotal());
    }
}
