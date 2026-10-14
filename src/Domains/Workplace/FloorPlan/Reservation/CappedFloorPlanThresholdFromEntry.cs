namespace Acme.Workforce.Workplace.FloorPlan.Reservation;

using Acme.Workforce.Workplace.Visitor.Entry;

/// <summary>Functions of capped floor plan threshold from entry.</summary>
public static class CappedFloorPlanThresholdFromEntry
{
    public static Dictionary<string, double> SummarizeWeightedVisitorEntriesForThreshold(List<WeightedVisitorEntry> items)
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

    public static List<CappedFloorPlanThreshold> DeriveCappedFloorPlanThresholdsFromEntry(List<WeightedVisitorEntry> items)
    {
        var outValue = new List<CappedFloorPlanThreshold>();
        foreach (var item in items)
        {
            var derived = CappedFloorPlanThreshold.NewCappedFloorPlanThreshold(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static double ResolveCappedFloorPlanThreshold(CappedFloorPlanThreshold item)
    {
        return PrimaryVisitorBatchPolicy.PrimaryVisitorBatchWeight(9, true);
    }
}
