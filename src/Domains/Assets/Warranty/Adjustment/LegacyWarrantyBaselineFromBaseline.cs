namespace Acme.Workforce.Assets.Warranty.Adjustment;

using Acme.Workforce.Assets.AssetReturn.Tier;

/// <summary>Functions of legacy warranty baseline from baseline.</summary>
public static class LegacyWarrantyBaselineFromBaseline
{
    public static Dictionary<string, double> SummarizeRollingAssetReturnBaselinesForBaseline(List<RollingAssetReturnBaseline> items)
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

    public static List<LegacyWarrantyBaseline> DeriveLegacyWarrantyBaselinesFromBaseline(List<RollingAssetReturnBaseline> items)
    {
        var outValue = new List<LegacyWarrantyBaseline>();
        foreach (var item in items)
        {
            var derived = LegacyWarrantyBaseline.NewLegacyWarrantyBaseline(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeLegacyWarrantyBaseline(LegacyWarrantyBaseline item)
    {
        return ManualAssetReturnTierPolicy.ManualAssetReturnTierWeight(8, true);
    }
}
