namespace Acme.Workforce.Assets.AssetReturn.Assignment;

using Acme.Workforce.Assets.AssetReturn.Cycle;

/// <summary>Functions of asset return assignment from adjustment.</summary>
public static class AssetReturnAssignmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeLenientAssetReturnAdjustmentsForAssignment(List<LenientAssetReturnAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<AssetReturnAssignment> DeriveAssetReturnAssignmentsFromAdjustment(List<LenientAssetReturnAdjustment> items)
    {
        var outValue = new List<AssetReturnAssignment>();
        foreach (var item in items)
        {
            var derived = AssetReturnAssignment.NewAssetReturnAssignment(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyAssetReturnAssignment(AssetReturnAssignment item)
    {
        return LenientAssetReturnAdjustmentPolicy.ClassifyLenientAssetReturnAdjustment(item.WeightedValue());
    }
}
