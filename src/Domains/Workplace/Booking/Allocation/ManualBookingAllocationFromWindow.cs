namespace Acme.Workforce.Workplace.Booking.Allocation;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Functions of manual booking allocation from window.</summary>
public static class ManualBookingAllocationFromWindow
{
    public static Dictionary<string, double> SummarizePrimaryBookingWindowsForAllocation(List<PrimaryBookingWindow> items)
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

    public static List<ManualBookingAllocation> DeriveManualBookingAllocationsFromWindow(List<PrimaryBookingWindow> items)
    {
        var outValue = new List<ManualBookingAllocation>();
        foreach (var item in items)
        {
            var derived = ManualBookingAllocation.NewManualBookingAllocation(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyManualBookingAllocation(ManualBookingAllocation item)
    {
        return PrimaryBookingAdjustmentPolicy.NormalizePrimaryBookingAdjustmentCode(item.Code);
    }
}
