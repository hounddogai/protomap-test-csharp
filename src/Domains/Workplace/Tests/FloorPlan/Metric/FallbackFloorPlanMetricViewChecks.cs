namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class FallbackFloorPlanMetricViewChecks
{
    public static void FallbackFloorPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = FallbackFloorPlanMetric.NewFallbackFloorPlanMetric("code-11", "label-899");
        var view = FallbackFloorPlanMetricView.ToFallbackFloorPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FallbackFloorPlanMetricView.ToFallbackFloorPlanMetricViews(new List<FallbackFloorPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FallbackFloorPlanMetricViewShowsTheStatusInCapitals();
    }
}
