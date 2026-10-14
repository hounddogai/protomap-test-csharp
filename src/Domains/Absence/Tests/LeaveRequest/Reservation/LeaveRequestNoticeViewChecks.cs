namespace Acme.Workforce.Absence.Tests.LeaveRequest.Reservation;

using Acme.Workforce.Absence.LeaveRequest.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LeaveRequestNoticeViewChecks
{
    public static void LeaveRequestNoticeViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestNotice.NewLeaveRequestNotice("code-764", "name-299");
        var view = LeaveRequestNoticeView.ToLeaveRequestNoticeView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestNoticeView.ToLeaveRequestNoticeViews(new List<LeaveRequestNotice> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestNoticeViewShowsTheStatusInCapitals();
    }
}
