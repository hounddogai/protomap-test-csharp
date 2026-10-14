namespace Acme.Workforce.Workplace.Occupancy.Checkpoint;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Functions of occupancy checkpoint from notice.</summary>
public static class OccupancyCheckpointFromNotice
{
    public static Dictionary<string, double> SummarizeFloorPlanNoticesForCheckpoint(List<FloorPlanNotice> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedTotal();
        }
        return totals;
    }

    public static List<OccupancyCheckpoint> DeriveOccupancyCheckpointsFromNotice(List<FloorPlanNotice> items)
    {
        var outValue = new List<OccupancyCheckpoint>();
        foreach (var item in items)
        {
            var derived = OccupancyCheckpoint.NewOccupancyCheckpoint(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double EstimateOccupancyCheckpoint(OccupancyCheckpoint item)
    {
        return FloorPlanCyclePolicy.FloorPlanCycleWeight(1, true);
    }
}
