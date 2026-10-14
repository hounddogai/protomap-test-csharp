namespace Acme.Workforce.Absence.Tests.LeaveRequest.Schedule;

using Acme.Workforce.Absence.LeaveRequest.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class LeaveRequestScheduleViewChecks
{
    public static void LeaveRequestScheduleViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestSchedule.NewLeaveRequestSchedule("code-293", "title-713");
        var view = LeaveRequestScheduleView.ToLeaveRequestScheduleView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestScheduleView.ToLeaveRequestScheduleViews(new List<LeaveRequestSchedule> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestScheduleViewShowsTheStatusInCapitals();
    }
}
