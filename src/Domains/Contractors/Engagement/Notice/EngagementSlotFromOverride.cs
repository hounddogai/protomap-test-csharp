namespace Acme.Workforce.Contractors.Engagement.Notice;

using Acme.Workforce.Contractors.Rate.Slot;

/// <summary>Functions of engagement slot from override.</summary>
public static class EngagementSlotFromOverride
{
    public static Dictionary<string, double> SummarizeManualRateOverridesForSlot(List<ManualRateOverride> items)
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

    public static List<EngagementSlot> DeriveEngagementSlotsFromOverride(List<ManualRateOverride> items)
    {
        var outValue = new List<EngagementSlot>();
        foreach (var item in items)
        {
            var derived = EngagementSlot.NewEngagementSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SplitEngagementSlot(EngagementSlot item)
    {
        return ScheduledRateAllocationPolicy.ScheduledRateAllocationBucket(item.EffectiveTotal());
    }
}
