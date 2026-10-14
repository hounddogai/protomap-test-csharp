namespace Acme.Workforce.Assets.Warranty.Bundle;

using Acme.Workforce.Assets.AssetReturn.Quota;

/// <summary>Functions of lenient warranty bundle from quota.</summary>
public static class LenientWarrantyBundleFromQuota
{
    public static Dictionary<string, double> SummarizeSeasonalAssetReturnQuotasForBundle(List<SeasonalAssetReturnQuota> items)
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

    public static List<LenientWarrantyBundle> DeriveLenientWarrantyBundlesFromQuota(List<SeasonalAssetReturnQuota> items)
    {
        var outValue = new List<LenientWarrantyBundle>();
        foreach (var item in items)
        {
            var derived = LenientWarrantyBundle.NewLenientWarrantyBundle(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeLenientWarrantyBundle(LenientWarrantyBundle item)
    {
        return AssetReturnClaimPolicy.AssetReturnClaimBucket(item.ProjectedExposure());
    }
}
