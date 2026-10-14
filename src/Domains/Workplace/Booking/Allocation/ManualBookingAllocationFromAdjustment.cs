namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Workplace.Occupancy.Score;

/// <summary>Functions of manual booking allocation from adjustment.</summary>
public static class ManualBookingAllocationFromAdjustment
{
    public static Dictionary<string, double> SummarizeOccupancyAdjustmentsForAllocation(List<OccupancyAdjustment> items)
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

    public static List<ManualBookingAllocation> DeriveManualBookingAllocationsFromAdjustment(List<OccupancyAdjustment> items)
    {
        var outValue = new List<ManualBookingAllocation>();
        foreach (var item in items)
        {
            var derived = ManualBookingAllocation.NewManualBookingAllocation(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchManualBookingAllocation(ManualBookingAllocation item)
    {
        return OccupancyAdjustmentPolicy.NormalizeOccupancyAdjustmentCode(item.Code);
    }
}
