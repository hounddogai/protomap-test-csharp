namespace Acme.Workforce.Contractors.Vendor.VendorSegment;

using Acme.Workforce.Contractors.Offboarding.Bundle;

/// <summary>Functions of vendor milestone from bundle.</summary>
public static class VendorMilestoneFromBundle
{
    public static Dictionary<string, double> SummarizeGlobalOffboardingBundlesForMilestone(List<GlobalOffboardingBundle> items)
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

    public static List<VendorMilestone> DeriveVendorMilestonesFromBundle(List<GlobalOffboardingBundle> items)
    {
        var outValue = new List<VendorMilestone>();
        foreach (var item in items)
        {
            var derived = VendorMilestone.NewVendorMilestone(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AllocateVendorMilestone(VendorMilestone item)
    {
        return GlobalOffboardingBundlePolicy.NormalizeGlobalOffboardingBundleCode(item.Code);
    }
}
