namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class FloorPlanOverrideViewChecks
{
    public static void FloorPlanOverrideViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanOverride.NewFloorPlanOverride("code-935", "label-215");
        var view = FloorPlanOverrideView.ToFloorPlanOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanOverrideView.ToFloorPlanOverrideViews(new List<FloorPlanOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanOverrideViewShowsTheStatusInCapitals();
    }
}
