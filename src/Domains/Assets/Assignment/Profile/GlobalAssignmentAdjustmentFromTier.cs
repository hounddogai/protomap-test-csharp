namespace Acme.Workforce.Assets.Assignment.Profile;

using Acme.Workforce.Assets.Warranty.Adjustment;

/// <summary>Functions of global assignment adjustment from tier.</summary>
public static class GlobalAssignmentAdjustmentFromTier
{
    public static Dictionary<string, double> SummarizeStrictWarrantyTiersForAdjustment(List<StrictWarrantyTier> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<GlobalAssignmentAdjustment> DeriveGlobalAssignmentAdjustmentsFromTier(List<StrictWarrantyTier> items)
    {
        var outValue = new List<GlobalAssignmentAdjustment>();
        foreach (var item in items)
        {
            var derived = GlobalAssignmentAdjustment.NewGlobalAssignmentAdjustment(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveGlobalAssignmentAdjustment(GlobalAssignmentAdjustment item)
    {
        return WarrantyAdjustmentPolicy.WarrantyAdjustmentBucket(item.AdjustedValue());
    }
}
