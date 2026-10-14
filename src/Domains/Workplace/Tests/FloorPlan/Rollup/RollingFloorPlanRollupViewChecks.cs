namespace Acme.Workforce.Workplace.Tests.FloorPlan.Rollup;

using Acme.Workforce.Workplace.FloorPlan.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class RollingFloorPlanRollupViewChecks
{
    public static void RollingFloorPlanRollupViewShowsTheStatusInCapitals()
    {
        var item = RollingFloorPlanRollup.NewRollingFloorPlanRollup("code-119", "name-745");
        var view = RollingFloorPlanRollupView.ToRollingFloorPlanRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingFloorPlanRollupView.ToRollingFloorPlanRollupViews(new List<RollingFloorPlanRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingFloorPlanRollupViewShowsTheStatusInCapitals();
    }
}
