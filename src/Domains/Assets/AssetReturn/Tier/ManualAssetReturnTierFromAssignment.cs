namespace Acme.Workforce.Assets.AssetReturn.Tier;

using Acme.Workforce.Assets.Refresh.OverrideItem;

/// <summary>Functions of manual asset return tier from assignment.</summary>
public static class ManualAssetReturnTierFromAssignment
{
    public static Dictionary<string, double> SummarizeBatchedRefreshAssignmentsForTier(List<BatchedRefreshAssignment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveValue();
        }
        return totals;
    }

    public static List<ManualAssetReturnTier> DeriveManualAssetReturnTiersFromAssignment(List<BatchedRefreshAssignment> items)
    {
        var outValue = new List<ManualAssetReturnTier>();
        foreach (var item in items)
        {
            var derived = ManualAssetReturnTier.NewManualAssetReturnTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveManualAssetReturnTier(ManualAssetReturnTier item)
    {
        return RefreshOverridePolicy.ClassifyRefreshOverride(item.NetLoad());
    }
}
