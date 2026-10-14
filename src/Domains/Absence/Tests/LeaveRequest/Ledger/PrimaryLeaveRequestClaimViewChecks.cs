namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestClaimViewChecks
{
    public static void PrimaryLeaveRequestClaimViewShowsTheStatusInCapitals()
    {
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("code-429", "name-211");
        var view = PrimaryLeaveRequestClaimView.ToPrimaryLeaveRequestClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(PrimaryLeaveRequestClaimView.ToPrimaryLeaveRequestClaimViews(new List<PrimaryLeaveRequestClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestClaimViewShowsTheStatusInCapitals();
    }
}
