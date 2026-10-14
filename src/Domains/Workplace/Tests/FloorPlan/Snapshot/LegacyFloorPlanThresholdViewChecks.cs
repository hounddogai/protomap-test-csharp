namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyFloorPlanThresholdViewChecks
{
    public static void LegacyFloorPlanThresholdViewShowsTheStatusInCapitals()
    {
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("code-663", "title-910");
        var view = LegacyFloorPlanThresholdView.ToLegacyFloorPlanThresholdView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LegacyFloorPlanThresholdView.ToLegacyFloorPlanThresholdViews(new List<LegacyFloorPlanThreshold> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LegacyFloorPlanThresholdViewShowsTheStatusInCapitals();
    }
}
