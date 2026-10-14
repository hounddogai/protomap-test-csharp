namespace Acme.Workforce.Workplace.Tests.FloorPlan.Metric;

using Acme.Workforce.Workplace.FloorPlan.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ManualFloorPlanMetricViewChecks
{
    public static void ManualFloorPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = ManualFloorPlanMetric.NewManualFloorPlanMetric("code-45", "label-706");
        var view = ManualFloorPlanMetricView.ToManualFloorPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ManualFloorPlanMetricView.ToManualFloorPlanMetricViews(new List<ManualFloorPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ManualFloorPlanMetricViewShowsTheStatusInCapitals();
    }
}
