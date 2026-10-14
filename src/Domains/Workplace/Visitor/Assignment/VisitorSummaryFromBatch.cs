namespace Acme.Workforce.Workplace.Visitor.Assignment;

using Acme.Workforce.Workplace.FloorPlan.Bundle;

/// <summary>Functions of visitor summary from batch.</summary>
public static class VisitorSummaryFromBatch
{
    public static Dictionary<string, double> SummarizeRollingFloorPlanBatchesForSummary(List<RollingFloorPlanBatch> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveLoad();
        }
        return totals;
    }

    public static List<VisitorSummary> DeriveVisitorSummariesFromBatch(List<RollingFloorPlanBatch> items)
    {
        var outValue = new List<VisitorSummary>();
        foreach (var item in items)
        {
            var derived = VisitorSummary.NewVisitorSummary(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string MatchVisitorSummary(VisitorSummary item)
    {
        return FloorPlanRequestPolicy.NormalizeFloorPlanRequestCode(item.Code);
    }
}
