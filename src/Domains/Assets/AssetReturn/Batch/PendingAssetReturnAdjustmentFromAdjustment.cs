namespace Acme.Workforce.Assets.AssetReturn.Batch;

using Acme.Workforce.Assets.Assignment.Policy;

/// <summary>Functions of pending asset return adjustment from adjustment.</summary>
public static class PendingAssetReturnAdjustmentFromAdjustment
{
    public static Dictionary<string, double> SummarizeFallbackAssignmentAdjustmentsForAdjustment(List<FallbackAssignmentAdjustment> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedValue();
        }
        return totals;
    }

    public static List<PendingAssetReturnAdjustment> DerivePendingAssetReturnAdjustmentsFromAdjustment(List<FallbackAssignmentAdjustment> items)
    {
        var outValue = new List<PendingAssetReturnAdjustment>();
        foreach (var item in items)
        {
            var derived = PendingAssetReturnAdjustment.NewPendingAssetReturnAdjustment(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputePendingAssetReturnAdjustment(PendingAssetReturnAdjustment item)
    {
        return AssignmentPreferencePolicy.AssignmentPreferenceBucket(item.EffectiveTotal());
    }
}
