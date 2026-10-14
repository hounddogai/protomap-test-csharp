namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestNoticeViewChecks
{
    public static void ScheduledLeaveRequestNoticeViewShowsTheStatusInCapitals()
    {
        var item = ScheduledLeaveRequestNotice.NewScheduledLeaveRequestNotice("code-240", "name-628");
        var view = ScheduledLeaveRequestNoticeView.ToScheduledLeaveRequestNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledLeaveRequestNoticeView.ToScheduledLeaveRequestNoticeViews(new List<ScheduledLeaveRequestNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestNoticeViewShowsTheStatusInCapitals();
    }
}
