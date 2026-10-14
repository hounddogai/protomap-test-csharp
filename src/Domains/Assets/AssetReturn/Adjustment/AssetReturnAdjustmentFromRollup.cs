namespace Acme.Workforce.Assets.AssetReturn.Adjustment;

using Acme.Workforce.Assets.Warranty.Agreement;

/// <summary>Functions of asset return adjustment from rollup.</summary>
public static class AssetReturnAdjustmentFromRollup
{
    public static Dictionary<string, double> SummarizeWarrantyRollupsForAdjustment(List<WarrantyRollup> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedVolume();
        }
        return totals;
    }

    public static List<AssetReturnAdjustment> DeriveAssetReturnAdjustmentsFromRollup(List<WarrantyRollup> items)
    {
        var outValue = new List<AssetReturnAdjustment>();
        foreach (var item in items)
        {
            var derived = AssetReturnAdjustment.NewAssetReturnAdjustment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double SummarizeAssetReturnAdjustment(AssetReturnAdjustment item)
    {
        return WarrantyEntryPolicy.WarrantyEntryWeight(9, true);
    }
}
