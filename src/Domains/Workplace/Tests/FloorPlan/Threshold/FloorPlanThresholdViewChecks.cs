namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanThresholdViewChecks
{
    public static void FloorPlanThresholdViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanThreshold.NewFloorPlanThreshold("code-250", "title-77");
        var view = FloorPlanThresholdView.ToFloorPlanThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanThresholdView.ToFloorPlanThresholdViews(new List<FloorPlanThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanThresholdViewShowsTheStatusInCapitals();
    }
}
