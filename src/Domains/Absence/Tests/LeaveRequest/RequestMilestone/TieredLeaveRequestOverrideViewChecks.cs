namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestMilestone;

using Acme.Workforce.Absence.LeaveRequest.RequestMilestone;

/// <summary>Checks of request milestone code, run by the project's runner.</summary>
public static class TieredLeaveRequestOverrideViewChecks
{
    public static void TieredLeaveRequestOverrideViewShowsTheStatusInCapitals()
    {
        var item = TieredLeaveRequestOverride.NewTieredLeaveRequestOverride("code-1", "title-262");
        var view = TieredLeaveRequestOverrideView.ToTieredLeaveRequestOverrideView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(TieredLeaveRequestOverrideView.ToTieredLeaveRequestOverrideViews(new List<TieredLeaveRequestOverride> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        TieredLeaveRequestOverrideViewShowsTheStatusInCapitals();
    }
}
