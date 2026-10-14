namespace Acme.Workforce.Workplace.FloorPlan.Adjustment;

using Acme.Workforce.Workplace.Desk.Milestone;

/// <summary>Functions of floor plan adjustment from checkpoint.</summary>
public static class FloorPlanAdjustmentFromCheckpoint
{
    public static Dictionary<string, double> SummarizeDeskCheckpointsForAdjustment(List<DeskCheckpoint> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.MeasuredValue();
        }
        return totals;
    }

    public static List<FloorPlanAdjustment> DeriveFloorPlanAdjustmentsFromCheckpoint(List<DeskCheckpoint> items)
    {
        var outValue = new List<FloorPlanAdjustment>();
        foreach (var item in items)
        {
            var derived = FloorPlanAdjustment.NewFloorPlanAdjustment(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ComputeFloorPlanAdjustment(FloorPlanAdjustment item)
    {
        return DeskCheckpointPolicy.DeskCheckpointWeight(8, true);
    }
}
