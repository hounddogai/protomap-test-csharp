namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTargetViewChecks
{
    public static void FloorPlanTargetViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanTarget.NewFloorPlanTarget("code-778", "title-971");
        var view = FloorPlanTargetView.ToFloorPlanTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanTargetView.ToFloorPlanTargetViews(new List<FloorPlanTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanTargetViewShowsTheStatusInCapitals();
    }
}
