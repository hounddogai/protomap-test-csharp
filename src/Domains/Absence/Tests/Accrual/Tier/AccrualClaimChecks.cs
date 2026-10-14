namespace Acme.Workforce.Absence.Tests.Accrual.Tier;

using Acme.Workforce.Absence.Accrual.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class AccrualClaimChecks
{
    public static void AccrualClaimStartsInPlanned()
    {
        var item = AccrualClaim.NewAccrualClaim("code-223", "label-612");
        Check.Equal(item.Status, AccrualClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void AccrualClaimMovesToCompleted()
    {
        var item = AccrualClaim.NewAccrualClaim("code-223", "label-612");
        try
        {
            item.MoveTo(AccrualClaimStatus.Completed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, AccrualClaimStatus.Completed, "status");
    }

    public static void AccrualClaimRejectsMovesFromCancelled()
    {
        var item = AccrualClaim.NewAccrualClaim("code-223", "label-612");
        item.Status = AccrualClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(AccrualClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        AccrualClaimStartsInPlanned();
        AccrualClaimMovesToCompleted();
        AccrualClaimRejectsMovesFromCancelled();
    }
}
