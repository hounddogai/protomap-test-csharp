namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LenientLeaveRequestClaimViewChecks
{
    public static void LenientLeaveRequestClaimViewShowsTheStatusInCapitals()
    {
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("code-286", "label-332");
        var view = LenientLeaveRequestClaimView.ToLenientLeaveRequestClaimView(item);
        Check.Equal(view.StatusLabel, $"{item.Status}".ToUpperInvariant(), "status label");
        Check.Equal(LenientLeaveRequestClaimView.ToLenientLeaveRequestClaimViews(new List<LenientLeaveRequestClaim> { item, item }).Count, 2, "views");
    }

    public static void RunAll()
    {
        LenientLeaveRequestClaimViewShowsTheStatusInCapitals();
    }
}
