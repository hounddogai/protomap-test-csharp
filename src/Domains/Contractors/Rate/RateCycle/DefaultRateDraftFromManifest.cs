namespace Acme.Workforce.Contractors.Rate.RateCycle;

using Acme.Workforce.Contractors.Vendor.VendorRequest;

/// <summary>Functions of default rate draft from manifest.</summary>
public static class DefaultRateDraftFromManifest
{
    public static Dictionary<string, double> SummarizeFallbackVendorManifestsForDraft(List<FallbackVendorManifest> items)
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

    public static List<DefaultRateDraft> DeriveDefaultRateDraftsFromManifest(List<FallbackVendorManifest> items)
    {
        var outValue = new List<DefaultRateDraft>();
        foreach (var item in items)
        {
            var derived = DefaultRateDraft.NewDefaultRateDraft(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyDefaultRateDraft(DefaultRateDraft item)
    {
        return FallbackVendorManifestPolicy.FallbackVendorManifestBucket(item.AdjustedValue());
    }
}
