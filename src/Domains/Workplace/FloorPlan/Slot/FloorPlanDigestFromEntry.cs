namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Workplace.Booking.Review;

/// <summary>Functions of floor plan digest from entry.</summary>
public static class FloorPlanDigestFromEntry
{
    public static Dictionary<string, double> SummarizeWeightedBookingEntriesForDigest(List<WeightedBookingEntry> items)
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

    public static List<FloorPlanDigest> DeriveFloorPlanDigestsFromEntry(List<WeightedBookingEntry> items)
    {
        var outValue = new List<FloorPlanDigest>();
        foreach (var item in items)
        {
            var derived = FloorPlanDigest.NewFloorPlanDigest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleFloorPlanDigest(FloorPlanDigest item)
    {
        return ScheduledBookingThresholdPolicy.ScheduledBookingThresholdWeight(7, true);
    }
}
