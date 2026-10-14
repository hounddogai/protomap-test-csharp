namespace Acme.Workforce.Compensation.Tests.BonusPool.Claim;

using Acme.Workforce.Compensation.BonusPool.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class LenientBonusPoolClaimChecks
{
    public static void LenientBonusPoolClaimStartsInPlanned()
    {
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("code-6", "name-514");
        Check.Equal(item.Status, LenientBonusPoolClaimStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LenientBonusPoolClaimMovesToInProgress()
    {
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("code-6", "name-514");
        try
        {
            item.MoveTo(LenientBonusPoolClaimStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LenientBonusPoolClaimStatus.InProgress, "status");
    }

    public static void LenientBonusPoolClaimRejectsMovesFromCancelled()
    {
        var item = LenientBonusPoolClaim.NewLenientBonusPoolClaim("code-6", "name-514");
        item.Status = LenientBonusPoolClaimStatus.Cancelled;
        Check.Fails(() => item.MoveTo(LenientBonusPoolClaimStatus.Planned), "move");
    }

    public static void RunAll()
    {
        LenientBonusPoolClaimStartsInPlanned();
        LenientBonusPoolClaimMovesToInProgress();
        LenientBonusPoolClaimRejectsMovesFromCancelled();
    }
}
