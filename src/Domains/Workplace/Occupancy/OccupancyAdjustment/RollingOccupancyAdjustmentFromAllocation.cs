namespace Acme.Workforce.Workplace.Occupancy.OccupancyAdjustment;

using Acme.Workforce.Workplace.Desk.DeskAllocation;

/// <summary>Functions of rolling occupancy adjustment from allocation.</summary>
public static class RollingOccupancyAdjustmentFromAllocation
{
    public static Dictionary<string, double> SummarizePendingDeskAllocationsForAdjustment(List<PendingDeskAllocation> items)
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

    public static List<RollingOccupancyAdjustment> DeriveRollingOccupancyAdjustmentsFromAllocation(List<PendingDeskAllocation> items)
    {
        var outValue = new List<RollingOccupancyAdjustment>();
        foreach (var item in items)
        {
            var derived = RollingOccupancyAdjustment.NewRollingOccupancyAdjustment(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveRollingOccupancyAdjustment(RollingOccupancyAdjustment item)
    {
        return TieredDeskCyclePolicy.TieredDeskCycleBucket(item.WeightedExposure());
    }
}
