namespace Acme.Workforce.Compensation.Tests.BonusPool.Window;

using Acme.Workforce.Compensation.BonusPool.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BonusPoolClaimChecks
{
    public static void BonusPoolClaimStartsInOpen()
    {
        var item = BonusPoolClaim.NewBonusPoolClaim("code-830", "name-111");
        Check.Equal(item.Status, BonusPoolClaimStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BonusPoolClaimMovesToResolved()
    {
        var item = BonusPoolClaim.NewBonusPoolClaim("code-830", "name-111");
        try
        {
            item.MoveTo(BonusPoolClaimStatus.Resolved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BonusPoolClaimStatus.Resolved, "status");
    }

    public static void BonusPoolClaimRejectsMovesFromArchived()
    {
        var item = BonusPoolClaim.NewBonusPoolClaim("code-830", "name-111");
        item.Status = BonusPoolClaimStatus.Archived;
        Check.Fails(() => item.MoveTo(BonusPoolClaimStatus.Open), "move");
    }

    public static void RunAll()
    {
        BonusPoolClaimStartsInOpen();
        BonusPoolClaimMovesToResolved();
        BonusPoolClaimRejectsMovesFromArchived();
    }
}
