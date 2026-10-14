namespace Acme.Workforce.Assets.Laptop.Baseline;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Functions of adaptive laptop manifest from group.</summary>
public static class AdaptiveLaptopManifestFromGroup
{
    public static Dictionary<string, double> SummarizeAssetReturnGroupsForManifest(List<AssetReturnGroup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetTotal();
        }
        return totals;
    }

    public static List<AdaptiveLaptopManifest> DeriveAdaptiveLaptopManifestsFromGroup(List<AssetReturnGroup> items)
    {
        var outValue = new List<AdaptiveLaptopManifest>();
        foreach (var item in items)
        {
            var derived = AdaptiveLaptopManifest.NewAdaptiveLaptopManifest(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeAdaptiveLaptopManifest(AdaptiveLaptopManifest item)
    {
        return AssetReturnAgreementPolicy.NormalizeAssetReturnAgreementCode(item.Code);
    }
}
