namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Functions of rolling occupancy adjustment from slot.</summary>
public static class RollingOccupancyAdjustmentFromSlot
{
    public static Dictionary<string, double> SummarizeBookingSlotsForAdjustment(List<BookingSlot> items)
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

    public static List<RollingOccupancyAdjustment> DeriveRollingOccupancyAdjustmentsFromSlot(List<BookingSlot> items)
    {
        var outValue = new List<RollingOccupancyAdjustment>();
        foreach (var item in items)
        {
            var derived = RollingOccupancyAdjustment.NewRollingOccupancyAdjustment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleRollingOccupancyAdjustment(RollingOccupancyAdjustment item)
    {
        return BookingSlotPolicy.BookingSlotWeight(3, true);
    }
}
