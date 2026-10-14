namespace Acme.Workforce.Workplace.FloorPlan.Item;

using Acme.Workforce.Workplace.AccessBadge.Ledger;

/// <summary>Functions of floor plan slot from milestone.</summary>
public static class FloorPlanSlotFromMilestone
{
    public static Dictionary<string, double> SummarizeManualAccessBadgeMilestonesForSlot(List<ManualAccessBadgeMilestone> items)
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

    public static List<FloorPlanSlot> DeriveFloorPlanSlotsFromMilestone(List<ManualAccessBadgeMilestone> items)
    {
        var outValue = new List<FloorPlanSlot>();
        foreach (var item in items)
        {
            var derived = FloorPlanSlot.NewFloorPlanSlot(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ClassifyFloorPlanSlotExtra(FloorPlanSlot item)
    {
        return ManualAccessBadgeMilestonePolicy.ClassifyManualAccessBadgeMilestone(item.WeightedValue());
    }
}
