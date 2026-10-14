namespace Acme.Workforce.Contractors.Vendor.VendorSchedule;

using Acme.Workforce.Contractors.Vendor.Schedule;

/// <summary>Functions of tiered vendor schedule from schedule.</summary>
public static class TieredVendorScheduleFromSchedule
{
    public static Dictionary<string, double> SummarizeScheduledVendorSchedulesForSchedule(List<ScheduledVendorSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<TieredVendorSchedule> DeriveTieredVendorSchedulesFromSchedule(List<ScheduledVendorSchedule> items)
    {
        var outValue = new List<TieredVendorSchedule>();
        foreach (var item in items)
        {
            var derived = TieredVendorSchedule.NewTieredVendorSchedule(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ReconcileTieredVendorSchedule(TieredVendorSchedule item)
    {
        return VendorEntryPolicy.VendorEntryWeight(5, true);
    }
}
