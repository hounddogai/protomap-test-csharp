namespace Acme.Workforce.Assets.Warranty.Milestone;

using Acme.Workforce.Assets.AssetReturn.Ledger;

/// <summary>Functions of warranty milestone from ledger.</summary>
public static class WarrantyMilestoneFromLedger
{
    public static Dictionary<string, double> SummarizeAssetReturnLedgersForMilestone(List<AssetReturnLedger> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedValue();
        }
        return totals;
    }

    public static List<WarrantyMilestone> DeriveWarrantyMilestonesFromLedger(List<AssetReturnLedger> items)
    {
        var outValue = new List<WarrantyMilestone>();
        foreach (var item in items)
        {
            var derived = WarrantyMilestone.NewWarrantyMilestone(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeWarrantyMilestone(WarrantyMilestone item)
    {
        return FallbackAssetReturnThresholdPolicy.FallbackAssetReturnThresholdBucket(item.NetExposure());
    }
}
