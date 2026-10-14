namespace Acme.Workforce.Workplace.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Occupancy.Target;

/// <summary>Functions of pending booking plan from segment.</summary>
public static class PendingBookingPlanFromSegment
{
    public static Dictionary<string, double> SummarizeStrictOccupancySegmentsForPlan(List<StrictOccupancySegment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<PendingBookingPlan> DerivePendingBookingPlansFromSegment(List<StrictOccupancySegment> items)
    {
        var outValue = new List<PendingBookingPlan>();
        foreach (var item in items)
        {
            var derived = PendingBookingPlan.NewPendingBookingPlan(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double BalancePendingBookingPlan(PendingBookingPlan item)
    {
        return OccupancyTargetPolicy.OccupancyTargetWeight(5, true);
    }
}
