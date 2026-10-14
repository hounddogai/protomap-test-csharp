namespace Acme.Workforce.Assets.Warranty.Allocation;

using Acme.Workforce.Assets.AssetReturn.Profile;

/// <summary>Functions of adaptive warranty milestone from profile.</summary>
public static class AdaptiveWarrantyMilestoneFromProfile
{
    public static Dictionary<string, double> SummarizeAssetReturnProfilesForMilestone(List<AssetReturnProfile> items)
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

    public static List<AdaptiveWarrantyMilestone> DeriveAdaptiveWarrantyMilestonesFromProfile(List<AssetReturnProfile> items)
    {
        var outValue = new List<AdaptiveWarrantyMilestone>();
        foreach (var item in items)
        {
            var derived = AdaptiveWarrantyMilestone.NewAdaptiveWarrantyMilestone(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignAdaptiveWarrantyMilestone(AdaptiveWarrantyMilestone item)
    {
        return StrictAssetReturnTierPolicy.NormalizeStrictAssetReturnTierCode(item.Code);
    }
}
