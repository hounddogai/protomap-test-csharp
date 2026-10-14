namespace Acme.Workforce.Absence.Tests.LeaveRequest.RequestClaim;

using Acme.Workforce.Absence.LeaveRequest.RequestClaim;

/// <summary>Checks of request claim code, run by the project's runner.</summary>
public static class LenientLeaveRequestClaimChecks
{
    public static void LenientLeaveRequestClaimStartsInOpen()
    {
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("code-286", "label-332");
        Check.Equal(item.Status, LenientLeaveRequestClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientLeaveRequestClaimMovesToInReview()
    {
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("code-286", "label-332");
        try
        {
            item.MoveTo(LenientLeaveRequestClaimStatus.InReview);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientLeaveRequestClaimStatus.InReview, "status");
    }

    public static void LenientLeaveRequestClaimRejectsMovesFromArchived()
    {
        var item = LenientLeaveRequestClaim.NewLenientLeaveRequestClaim("code-286", "label-332");
        item.Status = LenientLeaveRequestClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(LenientLeaveRequestClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        LenientLeaveRequestClaimStartsInOpen();
        LenientLeaveRequestClaimMovesToInReview();
        LenientLeaveRequestClaimRejectsMovesFromArchived();
    }
}
