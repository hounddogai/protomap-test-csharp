namespace Acme.Workforce.Workplace.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Functions of pending floor plan target from agreement.</summary>
public static class PendingFloorPlanTargetFromAgreement
{
    public static Dictionary<string, double> SummarizeCappedFloorPlanAgreementsForTarget(List<CappedFloorPlanAgreement> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.ProjectedExposure();
        }
        return totals;
    }

    public static List<PendingFloorPlanTarget> DerivePendingFloorPlanTargetsFromAgreement(List<CappedFloorPlanAgreement> items)
    {
        var outValue = new List<PendingFloorPlanTarget>();
        foreach (var item in items)
        {
            var derived = PendingFloorPlanTarget.NewPendingFloorPlanTarget(item.Code + "-next", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ProjectPendingFloorPlanTarget(PendingFloorPlanTarget item)
    {
        return FallbackFloorPlanMetricPolicy.ClassifyFallbackFloorPlanMetric(item.EffectiveTotal());
    }
}
