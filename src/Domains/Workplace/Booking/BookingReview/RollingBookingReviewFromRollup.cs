namespace Acme.Workforce.Workplace.Booking.BookingReview;

using Acme.Workforce.Workplace.Desk.Rule;

/// <summary>Functions of rolling booking review from rollup.</summary>
public static class RollingBookingReviewFromRollup
{
    public static Dictionary<string, double> SummarizePendingDeskRollupsForReview(List<PendingDeskRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedLoad();
        }
        return totals;
    }

    public static List<RollingBookingReview> DeriveRollingBookingReviewsFromRollup(List<PendingDeskRollup> items)
    {
        var outValue = new List<RollingBookingReview>();
        foreach (var item in items)
        {
            var derived = RollingBookingReview.NewRollingBookingReview(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ApplyRollingBookingReview(RollingBookingReview item)
    {
        return PendingDeskRollupPolicy.PendingDeskRollupWeight(1, true);
    }
}
