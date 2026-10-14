namespace Acme.Workforce.Workplace.Desk.Slot;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Functions of scheduled desk batch from target.</summary>
public static class ScheduledDeskBatchFromTarget
{
    public static Dictionary<string, double> SummarizeScheduledFloorPlanTargetsForBatch(List<ScheduledFloorPlanTarget> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.WeightedExposure();
        }
        return totals;
    }

    public static List<ScheduledDeskBatch> DeriveScheduledDeskBatchesFromTarget(List<ScheduledFloorPlanTarget> items)
    {
        var outValue = new List<ScheduledDeskBatch>();
        foreach (var item in items)
        {
            var derived = ScheduledDeskBatch.NewScheduledDeskBatch(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ApplyScheduledDeskBatch(ScheduledDeskBatch item)
    {
        return FloorPlanSlotPolicy.NormalizeFloorPlanSlotCode(item.Code);
    }
}
