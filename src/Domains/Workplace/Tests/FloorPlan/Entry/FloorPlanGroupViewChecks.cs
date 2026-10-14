namespace Acme.Workforce.Workplace.Tests.FloorPlan.Entry;

using Acme.Workforce.Workplace.FloorPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class FloorPlanGroupViewChecks
{
    public static void FloorPlanGroupViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanGroup.NewFloorPlanGroup("code-5", "label-78");
        var view = FloorPlanGroupView.ToFloorPlanGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanGroupView.ToFloorPlanGroupViews(new List<FloorPlanGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanGroupViewShowsTheStatusInCapitals();
    }
}
