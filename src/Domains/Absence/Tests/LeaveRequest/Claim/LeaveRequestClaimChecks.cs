namespace Acme.Workforce.Absence.Tests.LeaveRequest.Claim;

using Acme.Workforce.Absence.LeaveRequest.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LeaveRequestClaimChecks
{
    public static void LeaveRequestClaimStartsInCreated()
    {
        var item = LeaveRequestClaim.NewLeaveRequestClaim("code-662", "title-223");
        Check.Equal(item.Status, LeaveRequestClaimStatus.Created, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LeaveRequestClaimMovesToValidated()
    {
        var item = LeaveRequestClaim.NewLeaveRequestClaim("code-662", "title-223");
        try
        {
            item.MoveTo(LeaveRequestClaimStatus.Validated);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LeaveRequestClaimStatus.Validated, "status");
    }

    public static void LeaveRequestClaimRejectsMovesFromRetired()
    {
        var item = LeaveRequestClaim.NewLeaveRequestClaim("code-662", "title-223");
        item.Status = LeaveRequestClaimStatus.Retired;
        Check.Fails(() => item.MoveTo(LeaveRequestClaimStatus.Created), "move");
    }

    public static void RunAll()
    {
        LeaveRequestClaimStartsInCreated();
        LeaveRequestClaimMovesToValidated();
        LeaveRequestClaimRejectsMovesFromRetired();
    }
}
