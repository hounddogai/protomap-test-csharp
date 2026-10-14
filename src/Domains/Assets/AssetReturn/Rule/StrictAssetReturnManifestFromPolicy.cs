namespace Acme.Workforce.Assets.AssetReturn.Rule;

using Acme.Workforce.Assets.Warranty.Bundle;

/// <summary>Functions of strict asset return manifest from policy.</summary>
public static class StrictAssetReturnManifestFromPolicy
{
    public static Dictionary<string, double> SummarizeWarrantyPoliciesForManifest(List<WarrantyPolicy> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedLoad();
        }
        return totals;
    }

    public static List<StrictAssetReturnManifest> DeriveStrictAssetReturnManifestsFromPolicy(List<WarrantyPolicy> items)
    {
        var outValue = new List<StrictAssetReturnManifest>();
        foreach (var item in items)
        {
            var derived = StrictAssetReturnManifest.NewStrictAssetReturnManifest(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ProjectStrictAssetReturnManifest(StrictAssetReturnManifest item)
    {
        return BatchedWarrantyAgreementPolicy.BatchedWarrantyAgreementWeight(1, true);
    }
}
