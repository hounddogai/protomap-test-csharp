namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class ScheduledLeaveRequestRollupViewChecks
{
    public static void ScheduledLeaveRequestRollupViewShowsTheStatusInCapitals()
    {
        var item = ScheduledLeaveRequestRollup.NewScheduledLeaveRequestRollup("code-521", "label-500");
        var view = ScheduledLeaveRequestRollupView.ToScheduledLeaveRequestRollupView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(ScheduledLeaveRequestRollupView.ToScheduledLeaveRequestRollupViews(new List<ScheduledLeaveRequestRollup> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        ScheduledLeaveRequestRollupViewShowsTheStatusInCapitals();
    }
}
