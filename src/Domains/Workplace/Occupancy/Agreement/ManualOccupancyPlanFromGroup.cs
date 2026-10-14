namespace Acme.Workforce.Workplace.Occupancy.Agreement;

using Acme.Workforce.Workplace.Visitor.Group;

/// <summary>Functions of manual occupancy plan from group.</summary>
public static class ManualOccupancyPlanFromGroup
{
    public static Dictionary<string, double> SummarizeVisitorGroupsForPlan(List<VisitorGroup> items)
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

    public static List<ManualOccupancyPlan> DeriveManualOccupancyPlansFromGroup(List<VisitorGroup> items)
    {
        var outValue = new List<ManualOccupancyPlan>();
        foreach (var item in items)
        {
            var derived = ManualOccupancyPlan.NewManualOccupancyPlan(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ResolveManualOccupancyPlan(ManualOccupancyPlan item)
    {
        return VisitorGroupPolicy.VisitorGroupBucket(item.NetTotal());
    }
}
