namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanRollupViewChecks
{
    public static void FloorPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanRollup.NewFloorPlanRollup("code-685", "name-476");
        var view = FloorPlanRollupView.ToFloorPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanRollupView.ToFloorPlanRollupViews(new List<FloorPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanRollupViewShowsTheStatusInCapitals();
    }
}
