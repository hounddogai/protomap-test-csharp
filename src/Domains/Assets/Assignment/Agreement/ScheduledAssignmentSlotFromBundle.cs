namespace Acme.Workforce.Assets.Assignment.Agreement;

using Acme.Workforce.Assets.Warranty.Summary;

/// <summary>Functions of scheduled assignment slot from bundle.</summary>
public static class ScheduledAssignmentSlotFromBundle
{
    public static Dictionary<string, double> SummarizeWarrantyBundlesForSlot(List<WarrantyBundle> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetExposure();
        }
        return totals;
    }

    public static List<ScheduledAssignmentSlot> DeriveScheduledAssignmentSlotsFromBundle(List<WarrantyBundle> items)
    {
        var outValue = new List<ScheduledAssignmentSlot>();
        foreach (var item in items)
        {
            var derived = ScheduledAssignmentSlot.NewScheduledAssignmentSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyScheduledAssignmentSlot(ScheduledAssignmentSlot item)
    {
        return WarrantyOverridePolicy.WarrantyOverrideBucket(item.WeightedVolume());
    }
}
