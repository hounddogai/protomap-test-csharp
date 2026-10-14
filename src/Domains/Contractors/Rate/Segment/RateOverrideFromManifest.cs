namespace Acme.Workforce.Contractors.Rate.Segment;

using Acme.Workforce.Contractors.Vendor.VendorTemplate;

/// <summary>Functions of rate override from manifest.</summary>
public static class RateOverrideFromManifest
{
    public static Dictionary<string, double> SummarizePendingVendorManifestsForOverride(List<PendingVendorManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedTotal();
        }
        return totals;
    }

    public static List<RateOverride> DeriveRateOverridesFromManifest(List<PendingVendorManifest> items)
    {
        var outValue = new List<RateOverride>();
        foreach (var item in items)
        {
            var derived = RateOverride.NewRateOverride(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyRateOverride(RateOverride item)
    {
        return BatchedVendorTemplatePolicy.BatchedVendorTemplateBucket(item.ProjectedVolume());
    }
}
