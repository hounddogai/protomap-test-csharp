namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestGroupViewChecks
{
    public static void LeaveRequestGroupViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestGroup.NewLeaveRequestGroup("code-31", "name-777");
        var view = LeaveRequestGroupView.ToLeaveRequestGroupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestGroupView.ToLeaveRequestGroupViews(new List<LeaveRequestGroup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestGroupViewShowsTheStatusInCapitals();
    }
}
