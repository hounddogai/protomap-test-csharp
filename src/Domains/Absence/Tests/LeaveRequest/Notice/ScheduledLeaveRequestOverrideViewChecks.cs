namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestOverrideViewChecks
{
    public static void ScheduledLeaveRequestOverrideViewShowsTheStatusInCapitals()
    {
        var item = ScheduledLeaveRequestOverride.NewScheduledLeaveRequestOverride("code-968", "name-379");
        var view = ScheduledLeaveRequestOverrideView.ToScheduledLeaveRequestOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledLeaveRequestOverrideView.ToScheduledLeaveRequestOverrideViews(new List<ScheduledLeaveRequestOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestOverrideViewShowsTheStatusInCapitals();
    }
}
