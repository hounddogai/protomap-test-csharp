namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class ScheduledFloorPlanTargetViewChecks
{
    public static void ScheduledFloorPlanTargetViewShowsTheStatusInCapitals()
    {
        var item = ScheduledFloorPlanTarget.NewScheduledFloorPlanTarget("code-715", "label-187");
        var view = ScheduledFloorPlanTargetView.ToScheduledFloorPlanTargetView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledFloorPlanTargetView.ToScheduledFloorPlanTargetViews(new List<ScheduledFloorPlanTarget> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledFloorPlanTargetViewShowsTheStatusInCapitals();
    }
}
