namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestBaselineViewChecks
{
    public static void ScheduledLeaveRequestBaselineViewShowsTheStatusInCapitals()
    {
        var item = ScheduledLeaveRequestBaseline.NewScheduledLeaveRequestBaseline("code-100", "title-97");
        var view = ScheduledLeaveRequestBaselineView.ToScheduledLeaveRequestBaselineView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledLeaveRequestBaselineView.ToScheduledLeaveRequestBaselineViews(new List<ScheduledLeaveRequestBaseline> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestBaselineViewShowsTheStatusInCapitals();
    }
}
