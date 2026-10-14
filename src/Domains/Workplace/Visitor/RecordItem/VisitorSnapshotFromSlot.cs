namespace Acme.Workforce.Workplace.Visitor.RecordItem;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Functions of visitor snapshot from slot.</summary>
public static class VisitorSnapshotFromSlot
{
    public static Dictionary<string, double> SummarizeFloorPlanSlotsForSnapshot(List<FloorPlanSlot> items)
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

    public static List<VisitorSnapshot> DeriveVisitorSnapshotsFromSlot(List<FloorPlanSlot> items)
    {
        var outValue = new List<VisitorSnapshot>();
        foreach (var item in items)
        {
            var derived = VisitorSnapshot.NewVisitorSnapshot(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string EstimateVisitorSnapshot(VisitorSnapshot item)
    {
        return AdaptiveFloorPlanDraftPolicy.ClassifyAdaptiveFloorPlanDraft(item.AdjustedExposure());
    }
}
