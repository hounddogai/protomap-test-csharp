namespace Acme.Workforce.Absence.Tests.LeaveRequest.Review;

using Acme.Workforce.Absence.LeaveRequest.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class LeaveRequestQuotaViewChecks
{
    public static void LeaveRequestQuotaViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestQuota.NewLeaveRequestQuota("code-252", "name-154");
        var view = LeaveRequestQuotaView.ToLeaveRequestQuotaView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestQuotaView.ToLeaveRequestQuotaViews(new List<LeaveRequestQuota> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestQuotaViewShowsTheStatusInCapitals();
    }
}
