namespace Acme.Workforce.Workplace.AccessBadge.BadgeAudit;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Functions of strict access badge audit from group.</summary>
public static class StrictAccessBadgeAuditFromGroup
{
    public static Dictionary<string, double> SummarizeFloorPlanGroupsForAudit(List<FloorPlanGroup> items)
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

    public static List<StrictAccessBadgeAudit> DeriveStrictAccessBadgeAuditsFromGroup(List<FloorPlanGroup> items)
    {
        var outValue = new List<StrictAccessBadgeAudit>();
        foreach (var item in items)
        {
            var derived = StrictAccessBadgeAudit.NewStrictAccessBadgeAudit(item.Code + "-copy", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ComputeStrictAccessBadgeAudit(StrictAccessBadgeAudit item)
    {
        return WeightedFloorPlanMetricPolicy.ClassifyWeightedFloorPlanMetric(item.MeasuredValue());
    }
}
