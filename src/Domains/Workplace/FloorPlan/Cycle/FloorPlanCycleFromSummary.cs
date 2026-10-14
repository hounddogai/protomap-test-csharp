namespace Acme.Workforce.Workplace.FloorPlan.Cycle;

using Acme.Workforce.Workplace.Visitor.Summary;

/// <summary>Functions of floor plan cycle from summary.</summary>
public static class FloorPlanCycleFromSummary
{
    public static Dictionary<string, double> SummarizeManualVisitorSummariesForCycle(List<ManualVisitorSummary> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.NetLoad();
        }
        return totals;
    }

    public static List<FloorPlanCycle> DeriveFloorPlanCyclesFromSummary(List<ManualVisitorSummary> items)
    {
        var outValue = new List<FloorPlanCycle>();
        foreach (var item in items)
        {
            var derived = FloorPlanCycle.NewFloorPlanCycle(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string SettleFloorPlanCycle(FloorPlanCycle item)
    {
        return ManualVisitorSummaryPolicy.ClassifyManualVisitorSummary(item.NetValue());
    }
}
