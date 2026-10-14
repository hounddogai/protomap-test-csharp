namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class WeightedFloorPlanMetricViewChecks
{
    public static void WeightedFloorPlanMetricViewShowsTheStatusInCapitals()
    {
        var item = WeightedFloorPlanMetric.NewWeightedFloorPlanMetric("code-885", "label-710");
        var view = WeightedFloorPlanMetricView.ToWeightedFloorPlanMetricView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(WeightedFloorPlanMetricView.ToWeightedFloorPlanMetricViews(new List<WeightedFloorPlanMetric> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        WeightedFloorPlanMetricViewShowsTheStatusInCapitals();
    }
}
