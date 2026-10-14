namespace Acme.Workforce.Assets.Refresh.Preference;

using Acme.Workforce.Assets.AssetReturn.Agreement;

/// <summary>Functions of refresh milestone from group.</summary>
public static class RefreshMilestoneFromGroup
{
    public static Dictionary<string, double> SummarizeAssetReturnGroupsForMilestone(List<AssetReturnGroup> items)
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

    public static List<RefreshMilestone> DeriveRefreshMilestonesFromGroup(List<AssetReturnGroup> items)
    {
        var outValue = new List<RefreshMilestone>();
        foreach (var item in items)
        {
            var derived = RefreshMilestone.NewRefreshMilestone(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SummarizeRefreshMilestone(RefreshMilestone item)
    {
        return AssetReturnDigestPolicy.AssetReturnDigestBucket(item.WeightedTotal());
    }
}
