namespace Acme.Workforce.Workplace.Tests.FloorPlan.Notice;

using Acme.Workforce.Workplace.FloorPlan.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class RollingFloorPlanSegmentViewChecks
{
    public static void RollingFloorPlanSegmentViewShowsTheStatusInCapitals()
    {
        var item = RollingFloorPlanSegment.NewRollingFloorPlanSegment("code-61", "name-220");
        var view = RollingFloorPlanSegmentView.ToRollingFloorPlanSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(RollingFloorPlanSegmentView.ToRollingFloorPlanSegmentViews(new List<RollingFloorPlanSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        RollingFloorPlanSegmentViewShowsTheStatusInCapitals();
    }
}
