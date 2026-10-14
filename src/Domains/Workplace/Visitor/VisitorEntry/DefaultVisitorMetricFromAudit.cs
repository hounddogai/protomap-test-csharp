namespace Acme.Workforce.Workplace.Visitor.VisitorEntry;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Functions of default visitor metric from audit.</summary>
public static class DefaultVisitorMetricFromAudit
{
    public static Dictionary<string, double> SummarizeLenientFloorPlanAuditsForMetric(List<LenientFloorPlanAudit> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedVolume();
        }
        return totals;
    }

    public static List<DefaultVisitorMetric> DeriveDefaultVisitorMetricsFromAudit(List<LenientFloorPlanAudit> items)
    {
        var outValue = new List<DefaultVisitorMetric>();
        foreach (var item in items)
        {
            var derived = DefaultVisitorMetric.NewDefaultVisitorMetric(item.Code + "-copy", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string DeriveDefaultVisitorMetric(DefaultVisitorMetric item)
    {
        return StrictFloorPlanEntryPolicy.StrictFloorPlanEntryBucket(item.ProjectedExposure());
    }
}
