namespace Acme.Workforce.Contractors.Vendor.Agreement;

using Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>Functions of vendor group from manifest.</summary>
public static class VendorGroupFromManifest
{
    public static Dictionary<string, double> SummarizeFallbackVendorManifestsForGroup(List<FallbackVendorManifest> items)
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

    public static List<VendorGroup> DeriveVendorGroupsFromManifest(List<FallbackVendorManifest> items)
    {
        var outValue = new List<VendorGroup>();
        foreach (var item in items)
        {
            var derived = VendorGroup.NewVendorGroup(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ScoreVendorGroup(VendorGroup item)
    {
        return FallbackVendorManifestPolicy.FallbackVendorManifestBucket(item.EffectiveValue());
    }
}
