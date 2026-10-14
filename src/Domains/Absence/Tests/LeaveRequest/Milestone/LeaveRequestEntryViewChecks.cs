namespace Acme.Workforce.Absence.Tests.LeaveRequest.Milestone;

using Acme.Workforce.Absence.LeaveRequest.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class LeaveRequestEntryViewChecks
{
    public static void LeaveRequestEntryViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestEntry.NewLeaveRequestEntry("code-903", "name-696");
        var view = LeaveRequestEntryView.ToLeaveRequestEntryView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestEntryView.ToLeaveRequestEntryViews(new List<LeaveRequestEntry> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestEntryViewShowsTheStatusInCapitals();
    }
}
