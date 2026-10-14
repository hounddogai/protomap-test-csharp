namespace Acme.Workforce.Assets.Warranty.Metric;

using Acme.Workforce.Assets.AssetReturn.Summary;

/// <summary>Functions of tiered warranty override from schedule.</summary>
public static class TieredWarrantyOverrideFromSchedule
{
    public static Dictionary<string, double> SummarizeAssetReturnSchedulesForOverride(List<AssetReturnSchedule> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedTotal();
        }
        return totals;
    }

    public static List<TieredWarrantyOverride> DeriveTieredWarrantyOverridesFromSchedule(List<AssetReturnSchedule> items)
    {
        var outValue = new List<TieredWarrantyOverride>();
        foreach (var item in items)
        {
            var derived = TieredWarrantyOverride.NewTieredWarrantyOverride(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitTieredWarrantyOverride(TieredWarrantyOverride item)
    {
        return ScheduledAssetReturnProposalPolicy.ClassifyScheduledAssetReturnProposal(item.NetTotal());
    }
}
