namespace Acme.Workforce.Assets.AssetReturn.Variance;

using Acme.Workforce.Assets.Laptop.Manifest;

/// <summary>Functions of asset return variance from manifest.</summary>
public static class AssetReturnVarianceFromManifest
{
    public static Dictionary<string, double> SummarizeLaptopManifestsForVariance(List<LaptopManifest> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedVolume();
        }
        return totals;
    }

    public static List<AssetReturnVariance> DeriveAssetReturnVariancesFromManifest(List<LaptopManifest> items)
    {
        var outValue = new List<AssetReturnVariance>();
        foreach (var item in items)
        {
            var derived = AssetReturnVariance.NewAssetReturnVariance(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileAssetReturnVariance(AssetReturnVariance item)
    {
        return TieredLaptopTierPolicy.ClassifyTieredLaptopTier(item.AdjustedLoad());
    }
}
