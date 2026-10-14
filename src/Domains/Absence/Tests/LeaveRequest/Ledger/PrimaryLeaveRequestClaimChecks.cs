namespace Acme.Workforce.Absence.Tests.LeaveRequest.Ledger;

using Acme.Workforce.Absence.LeaveRequest.Ledger;

/// <summary>Checks of ledger code, run by the project's runner.</summary>
public static class PrimaryLeaveRequestClaimChecks
{
    public static void PrimaryLeaveRequestClaimStartsInRequested()
    {
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("code-429", "name-211");
        Check.Equal(item.Status, PrimaryLeaveRequestClaimStatus.Requested, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryLeaveRequestClaimMovesToGranted()
    {
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("code-429", "name-211");
        try
        {
            item.MoveTo(PrimaryLeaveRequestClaimStatus.Granted);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryLeaveRequestClaimStatus.Granted, "status");
    }

    public static void PrimaryLeaveRequestClaimRejectsMovesFromExpired()
    {
        var item = PrimaryLeaveRequestClaim.NewPrimaryLeaveRequestClaim("code-429", "name-211");
        item.Status = PrimaryLeaveRequestClaimStatus.Expired;
        Check.Fails(() => item.MoveTo(PrimaryLeaveRequestClaimStatus.Requested), "move");
    }

    public static void RunAll()
    {
        PrimaryLeaveRequestClaimStartsInRequested();
        PrimaryLeaveRequestClaimMovesToGranted();
        PrimaryLeaveRequestClaimRejectsMovesFromExpired();
    }
}
