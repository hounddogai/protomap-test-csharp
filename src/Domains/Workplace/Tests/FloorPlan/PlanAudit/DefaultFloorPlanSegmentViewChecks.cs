namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanAudit;

using Acme.Workforce.Workplace.FloorPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class DefaultFloorPlanSegmentViewChecks
{
    public static void DefaultFloorPlanSegmentViewShowsTheStatusInCapitals()
    {
        var item = DefaultFloorPlanSegment.NewDefaultFloorPlanSegment("code-349", "name-966");
        var view = DefaultFloorPlanSegmentView.ToDefaultFloorPlanSegmentView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(DefaultFloorPlanSegmentView.ToDefaultFloorPlanSegmentViews(new List<DefaultFloorPlanSegment> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        DefaultFloorPlanSegmentViewShowsTheStatusInCapitals();
    }
}
