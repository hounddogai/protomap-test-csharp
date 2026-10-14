namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAdjustment;

using Acme.Workforce.Workplace.FloorPlan.PlanAdjustment;

/// <summary>Checks of plan adjustment code, run by the project's runner.</summary>
public static class FloorPlanMetricViewChecks
{
    public static void FloorPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanMetric.NewFloorPlanMetric("code-596", "name-36");
        var view = FloorPlanMetricView.ToFloorPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanMetricView.ToFloorPlanMetricViews(new List<FloorPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanMetricViewShowsTheStatusInCapitals();
    }
}
