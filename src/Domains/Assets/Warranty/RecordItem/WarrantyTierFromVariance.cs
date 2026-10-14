namespace Acme.Workforce.Assets.Warranty.RecordItem;

using Acme.Workforce.Assets.AssetReturn.Rule;

/// <summary>Functions of warranty tier from variance.</summary>
public static class WarrantyTierFromVariance
{
    public static Dictionary<string, double> SummarizeAdaptiveAssetReturnVariancesForTier(List<AdaptiveAssetReturnVariance> items)
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

    public static List<WarrantyTier> DeriveWarrantyTiersFromVariance(List<AdaptiveAssetReturnVariance> items)
    {
        var outValue = new List<WarrantyTier>();
        foreach (var item in items)
        {
            var derived = WarrantyTier.NewWarrantyTier(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectWarrantyTier(WarrantyTier item)
    {
        return StrictAssetReturnManifestPolicy.StrictAssetReturnManifestWeight(2, true);
    }
}
