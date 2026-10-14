namespace Acme.Workforce.Workplace.Visitor.Slot;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Functions of visitor slot from adjustment.</summary>
public static class VisitorSlotFromAdjustment
{
    public static Dictionary<string, double> SummarizeFloorPlanAdjustmentsForSlot(List<FloorPlanAdjustment> items)
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

    public static List<VisitorSlot> DeriveVisitorSlotsFromAdjustment(List<FloorPlanAdjustment> items)
    {
        var outValue = new List<VisitorSlot>();
        foreach (var item in items)
        {
            var derived = VisitorSlot.NewVisitorSlot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double AllocateVisitorSlot(VisitorSlot item)
    {
        return FloorPlanCheckpointPolicy.FloorPlanCheckpointWeight(7, true);
    }
}
