namespace Acme.Workforce.Workplace.FloorPlan.Baseline;

using Acme.Workforce.Workplace.Visitor.Estimate;

/// <summary>Functions of floor plan estimate from estimate.</summary>
public static class FloorPlanEstimateFromEstimate
{
    public static Dictionary<string, double> SummarizeVisitorEstimatesForEstimate(List<VisitorEstimate> items)
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

    public static List<FloorPlanEstimate> DeriveFloorPlanEstimatesFromEstimate(List<VisitorEstimate> items)
    {
        var outValue = new List<FloorPlanEstimate>();
        foreach (var item in items)
        {
            var derived = FloorPlanEstimate.NewFloorPlanEstimate(item.Code + "-next", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string AssignFloorPlanEstimate(FloorPlanEstimate item)
    {
        return PendingVisitorPolicyPolicy.ClassifyPendingVisitorPolicy(item.NetVolume());
    }
}
