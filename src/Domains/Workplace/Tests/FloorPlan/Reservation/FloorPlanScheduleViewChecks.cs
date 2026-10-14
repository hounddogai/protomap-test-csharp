namespace Acme.Workforce.Workplace.Tests.FloorPlan.Reservation;

using Acme.Workforce.Workplace.FloorPlan.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class FloorPlanScheduleViewChecks
{
    public static void FloorPlanScheduleViewShowsTheStatusInCapitals()
    {
        var item = FloorPlanSchedule.NewFloorPlanSchedule("code-891", "name-249");
        var view = FloorPlanScheduleView.ToFloorPlanScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(FloorPlanScheduleView.ToFloorPlanScheduleViews(new List<FloorPlanSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        FloorPlanScheduleViewShowsTheStatusInCapitals();
    }
}
