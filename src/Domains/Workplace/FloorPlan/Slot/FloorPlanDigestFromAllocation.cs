namespace Acme.Workforce.Workplace.FloorPlan.Slot;

using Acme.Workforce.Workplace.Desk.Variance;

/// <summary>Functions of floor plan digest from allocation.</summary>
public static class FloorPlanDigestFromAllocation
{
    public static Dictionary<string, double> SummarizeLenientDeskAllocationsForDigest(List<LenientDeskAllocation> items)
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

    public static List<FloorPlanDigest> DeriveFloorPlanDigestsFromAllocation(List<LenientDeskAllocation> items)
    {
        var outValue = new List<FloorPlanDigest>();
        foreach (var item in items)
        {
            var derived = FloorPlanDigest.NewFloorPlanDigest(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string BalanceFloorPlanDigest(FloorPlanDigest item)
    {
        return AdaptiveDeskBatchPolicy.ClassifyAdaptiveDeskBatch(item.AdjustedVolume());
    }
}
