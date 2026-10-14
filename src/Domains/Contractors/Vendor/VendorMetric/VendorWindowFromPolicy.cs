namespace Acme.Workforce.Contractors.Vendor.VendorMetric;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Functions of vendor window from policy.</summary>
public static class VendorWindowFromPolicy
{
    public static Dictionary<string, double> SummarizeCappedRatePoliciesForWindow(List<CappedRatePolicy> items)
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

    public static List<VendorWindow> DeriveVendorWindowsFromPolicy(List<CappedRatePolicy> items)
    {
        var outValue = new List<VendorWindow>();
        foreach (var item in items)
        {
            var derived = VendorWindow.NewVendorWindow(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignVendorWindow(VendorWindow item)
    {
        return ScheduledRateAllocationPolicy.ScheduledRateAllocationBucket(item.NetValue());
    }
}
