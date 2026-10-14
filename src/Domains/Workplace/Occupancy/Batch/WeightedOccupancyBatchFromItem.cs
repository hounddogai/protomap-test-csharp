namespace Acme.Workforce.Workplace.Occupancy.Batch;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Functions of weighted occupancy batch from item.</summary>
public static class WeightedOccupancyBatchFromItem
{
    public static Dictionary<string, double> SummarizeFloorPlanItemsForBatch(List<FloorPlanItem> items)
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

    public static List<WeightedOccupancyBatch> DeriveWeightedOccupancyBatchesFromItem(List<FloorPlanItem> items)
    {
        var outValue = new List<WeightedOccupancyBatch>();
        foreach (var item in items)
        {
            var derived = WeightedOccupancyBatch.NewWeightedOccupancyBatch(item.Code + "-draft", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MergeWeightedOccupancyBatch(WeightedOccupancyBatch item)
    {
        return FloorPlanItemPolicy.ClassifyFloorPlanItem(item.NetValue());
    }
}
