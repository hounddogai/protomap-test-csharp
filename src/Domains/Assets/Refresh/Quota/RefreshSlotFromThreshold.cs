namespace Acme.Workforce.Assets.Refresh.Quota;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Functions of refresh slot from threshold.</summary>
public static class RefreshSlotFromThreshold
{
    public static Dictionary<string, double> SummarizeFallbackAssetReturnThresholdsForSlot(List<FallbackAssetReturnThreshold> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedExposure();
        }
        return totals;
    }

    public static List<RefreshSlot> DeriveRefreshSlotsFromThreshold(List<FallbackAssetReturnThreshold> items)
    {
        var outValue = new List<RefreshSlot>();
        foreach (var item in items)
        {
            var derived = RefreshSlot.NewRefreshSlot(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitRefreshSlot(RefreshSlot item)
    {
        return AssetReturnSegmentPolicy.AssetReturnSegmentBucket(item.WeightedVolume());
    }
}
