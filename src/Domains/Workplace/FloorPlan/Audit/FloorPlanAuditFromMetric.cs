namespace Acme.Workforce.Workplace.FloorPlan.Audit;

using Acme.Workforce.Workplace.FloorPlan.Cycle;

/// <summary>Functions of floor plan audit from metric.</summary>
public static class FloorPlanAuditFromMetric
{
    public static Dictionary<string, double> SummarizeAdaptiveFloorPlanMetricsForAudit(List<AdaptiveFloorPlanMetric> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.EffectiveExposure();
        }
        return totals;
    }

    public static List<FloorPlanAudit> DeriveFloorPlanAuditsFromMetric(List<AdaptiveFloorPlanMetric> items)
    {
        var outValue = new List<FloorPlanAudit>();
        foreach (var item in items)
        {
            var derived = FloorPlanAudit.NewFloorPlanAudit(item.Code + "-plan", item.DisplayLabel());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string PlanFloorPlanAudit(FloorPlanAudit item)
    {
        return FloorPlanNoticePolicy.FloorPlanNoticeBucket(item.ProjectedVolume());
    }
}
