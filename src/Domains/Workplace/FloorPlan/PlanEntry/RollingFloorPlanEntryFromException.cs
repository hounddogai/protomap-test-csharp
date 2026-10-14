namespace Acme.Workforce.Workplace.FloorPlan.PlanEntry;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Functions of rolling floor plan entry from exception.</summary>
public static class RollingFloorPlanEntryFromException
{
    public static Dictionary<string, double> SummarizeLenientBookingExceptionsForEntry(List<LenientBookingException> items)
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

    public static List<RollingFloorPlanEntry> DeriveRollingFloorPlanEntriesFromException(List<LenientBookingException> items)
    {
        var outValue = new List<RollingFloorPlanEntry>();
        foreach (var item in items)
        {
            var derived = RollingFloorPlanEntry.NewRollingFloorPlanEntry(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleRollingFloorPlanEntry(RollingFloorPlanEntry item)
    {
        return RollingBookingReviewPolicy.ClassifyRollingBookingReview(item.ProjectedValue());
    }
}
