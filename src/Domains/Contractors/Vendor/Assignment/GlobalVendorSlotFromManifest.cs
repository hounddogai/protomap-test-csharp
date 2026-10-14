namespace Acme.Workforce.Contractors.Vendor.Assignment;

using Acme.Workforce.Contractors.Offboarding.Threshold;

/// <summary>Functions of global vendor slot from manifest.</summary>
public static class GlobalVendorSlotFromManifest
{
    public static Dictionary<string, double> SummarizeOffboardingManifestsForSlot(List<OffboardingManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<GlobalVendorSlot> DeriveGlobalVendorSlotsFromManifest(List<OffboardingManifest> items)
    {
        var outValue = new List<GlobalVendorSlot>();
        foreach (var item in items)
        {
            var derived = GlobalVendorSlot.NewGlobalVendorSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceGlobalVendorSlot(GlobalVendorSlot item)
    {
        return OffboardingManifestPolicy.ClassifyOffboardingManifest(item.MeasuredValue());
    }
}
