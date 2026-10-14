namespace Acme.Workforce.Workplace.FloorPlan.Ledger;

using Acme.Workforce.Workplace.FloorPlan.Adjustment;

/// <summary>Functions of floor plan batch from reservation.</summary>
public static class FloorPlanBatchFromReservation
{
    public static Dictionary<string, double> SummarizeFloorPlanReservationsForBatch(List<FloorPlanReservation> items)
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

    public static List<FloorPlanBatch> DeriveFloorPlanBatchesFromReservation(List<FloorPlanReservation> items)
    {
        var outValue = new List<FloorPlanBatch>();
        foreach (var item in items)
        {
            var derived = FloorPlanBatch.NewFloorPlanBatch(item.Code + "-draft", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignFloorPlanBatch(FloorPlanBatch item)
    {
        return FloorPlanRecordPolicy.FloorPlanRecordBucket(item.WeightedTotal());
    }
}
