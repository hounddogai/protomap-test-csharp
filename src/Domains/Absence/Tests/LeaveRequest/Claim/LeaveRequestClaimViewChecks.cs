namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LeaveRequestClaimViewChecks
{
    public static void LeaveRequestClaimViewShowsTheStatusInCapitals()
    {
        var item = LeaveRequestClaim.NewLeaveRequestClaim("code-662", "title-223");
        var view = LeaveRequestClaimView.ToLeaveRequestClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LeaveRequestClaimView.ToLeaveRequestClaimViews(new List<LeaveRequestClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LeaveRequestClaimViewShowsTheStatusInCapitals();
    }
}
