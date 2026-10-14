namespace Acme.Workforce.Absence.Tests.LeaveRequest.Baseline;

using Acme.Workforce.Absence.LeaveRequest.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class LeaveRequestOverrideViewChecks
{
    public static void LeaveRequestOverrideViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestOverride.NewLeaveRequestOverride("code-590", "label-209");
        var view = LeaveRequestOverrideView.ToLeaveRequestOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestOverrideView.ToLeaveRequestOverrideViews(new List<LeaveRequestOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestOverrideViewShowsTheStatusInCapitals();
    }
}
