namespace Acme.Workforce.Orgdesign.Tests.Headcount.Adjustment;

using Acme.Workforce.Orgdesign.Headcount.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class HeadcountClaimChecks
{
    public static void HeadcountClaimStartsInProposed()
    {
        var item = HeadcountClaim.NewHeadcountClaim("code-248", "title-690");
        Check.Equal(item.Status, HeadcountClaimStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountClaimMovesToWithdrawn()
    {
        var item = HeadcountClaim.NewHeadcountClaim("code-248", "title-690");
        try
        {
            item.MoveTo(HeadcountClaimStatus.Withdrawn);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountClaimStatus.Withdrawn, "status");
    }

    public static void HeadcountClaimRejectsMovesFromWithdrawn()
    {
        var item = HeadcountClaim.NewHeadcountClaim("code-248", "title-690");
        item.Status = HeadcountClaimStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(HeadcountClaimStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        HeadcountClaimStartsInProposed();
        HeadcountClaimMovesToWithdrawn();
        HeadcountClaimRejectsMovesFromWithdrawn();
    }
}
