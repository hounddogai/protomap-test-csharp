namespace Acme.Workforce.Contractors.Vendor.Metric;

using Acme.Workforce.Contractors.Vendor.VendorSchedule;

/// <summary>Functions of vendor bundle from schedule.</summary>
public static class VendorBundleFromSchedule
{
    public static Dictionary<string, double> SummarizeTieredVendorSchedulesForBundle(List<TieredVendorSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveVolume();
        }
        return totals;
    }

    public static List<VendorBundle> DeriveVendorBundlesFromSchedule(List<TieredVendorSchedule> items)
    {
        var outValue = new List<VendorBundle>();
        foreach (var item in items)
        {
            var derived = VendorBundle.NewVendorBundle(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double DeriveVendorBundle(VendorBundle item)
    {
        return TieredVendorSchedulePolicy.TieredVendorScheduleWeight(4, true);
    }
}
