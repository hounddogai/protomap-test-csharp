namespace Acme.Workforce.Assets.AssetReturn.OverrideItem;

using Acme.Workforce.Assets.Assignment.Claim;

/// <summary>Functions of asset return tier from claim.</summary>
public static class AssetReturnTierFromClaim
{
    public static Dictionary<string, double> SummarizeAssignmentClaimsForTier(List<AssignmentClaim> items)
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

    public static List<AssetReturnTier> DeriveAssetReturnTiersFromClaim(List<AssignmentClaim> items)
    {
        var outValue = new List<AssetReturnTier>();
        foreach (var item in items)
        {
            var derived = AssetReturnTier.NewAssetReturnTier(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SettleAssetReturnTier(AssetReturnTier item)
    {
        return AssignmentSchedulePolicy.AssignmentScheduleWeight(3, true);
    }
}
