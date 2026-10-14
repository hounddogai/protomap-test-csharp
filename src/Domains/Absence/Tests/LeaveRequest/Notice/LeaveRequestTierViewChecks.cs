namespace Acme.Workforce.Absence.Tests.LeaveRequest.Notice;

using Acme.Workforce.Absence.LeaveRequest.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class LeaveRequestTierViewChecks
{
    public static void LeaveRequestTierViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestTier.NewLeaveRequestTier("code-991", "label-294");
        var view = LeaveRequestTierView.ToLeaveRequestTierView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestTierView.ToLeaveRequestTierViews(new List<LeaveRequestTier> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestTierViewShowsTheStatusInCapitals();
    }
}
