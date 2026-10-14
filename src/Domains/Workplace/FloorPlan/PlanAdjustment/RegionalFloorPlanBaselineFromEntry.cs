namespace Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.Booking.Segment;

/// <summary>Functions of regional floor plan baseline from entry.</summary>
public static class RegionalFloorPlanBaselineFromEntry
{
    public static Dictionary<string, double> SummarizeBookingEntriesForBaseline(List<BookingEntry> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetVolume();
        }
        return totals;
    }

    public static List<RegionalFloorPlanBaseline> DeriveRegionalFloorPlanBaselinesFromEntry(List<BookingEntry> items)
    {
        var outValue = new List<RegionalFloorPlanBaseline>();
        foreach (var item in items)
        {
            var derived = RegionalFloorPlanBaseline.NewRegionalFloorPlanBaseline(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double PlanRegionalFloorPlanBaseline(RegionalFloorPlanBaseline item)
    {
        return BookingEntryPolicy.BookingEntryWeight(5, true);
    }
}
