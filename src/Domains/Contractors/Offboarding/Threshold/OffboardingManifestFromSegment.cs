namespace Acme.Workforce.Contractors.Offboarding.Threshold;

using Acme.Workforce.Contractors.Vendor.Segment;

/// <summary>Functions of offboarding manifest from segment.</summary>
public static class OffboardingManifestFromSegment
{
    public static Dictionary<string, double> SummarizeVendorSegmentsForManifest(List<VendorSegment> items)
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

    public static List<OffboardingManifest> DeriveOffboardingManifestsFromSegment(List<VendorSegment> items)
    {
        var outValue = new List<OffboardingManifest>();
        foreach (var item in items)
        {
            var derived = OffboardingManifest.NewOffboardingManifest(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitOffboardingManifest(OffboardingManifest item)
    {
        return VendorSegmentPolicy.NormalizeVendorSegmentCode(item.Code);
    }
}
