namespace Acme.Workforce.Workplace.FloorPlan.Allocation;

using Acme.Workforce.Workplace.Booking.Baseline;

/// <summary>Functions of floor plan assignment from adjustment.</summary>
public static class FloorPlanAssignmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeBookingAdjustmentsForAssignment(List<BookingAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<FloorPlanAssignment> DeriveFloorPlanAssignmentsFromAdjustment(List<BookingAdjustment> items)
    {
        var outValue = new List<FloorPlanAssignment>();
        foreach (var item in items)
        {
            var derived = FloorPlanAssignment.NewFloorPlanAssignment(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string NormalizeFloorPlanAssignment(FloorPlanAssignment item)
    {
        return BookingAdjustmentPolicy.ClassifyBookingAdjustment(item.AdjustedExposure());
    }
}
