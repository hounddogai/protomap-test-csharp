namespace Acme.Workforce.Workplace.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.Visitor.Review;

/// <summary>Functions of floor plan override from review.</summary>
public static class FloorPlanOverrideFromReview
{
    public static Dictionary<string, double> SummarizeVisitorReviewsForOverride(List<VisitorReview> items)
    {
        var totals = new Dictionary<string, double>();
        foreach (var item in items)
        {
            if (item.IsTerminal())
            {
                continue;
            }
            totals[item.GroupKey()] = totals.GetValueOrDefault(item.GroupKey()) + item.AdjustedLoad();
        }
        return totals;
    }

    public static List<FloorPlanOverride> DeriveFloorPlanOverridesFromReview(List<VisitorReview> items)
    {
        var outValue = new List<FloorPlanOverride>();
        foreach (var item in items)
        {
            var derived = FloorPlanOverride.NewFloorPlanOverride(item.Code + "-plan", item.Label());
            outValue.Add(derived);
        }
        return outValue;
    }

    public static string ReconcileFloorPlanOverride(FloorPlanOverride item)
    {
        return LegacyVisitorMilestonePolicy.ClassifyLegacyVisitorMilestone(item.ProjectedVolume());
    }
}
