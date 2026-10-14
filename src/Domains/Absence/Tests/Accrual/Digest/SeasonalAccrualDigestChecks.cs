namespace Acme.Workforce.Absence.Tests.Accrual.Digest;

using Acme.Workforce.Absence.Accrual.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class SeasonalAccrualDigestChecks
{
    public static void SeasonalAccrualDigestStartsInProposed()
    {
        var item = SeasonalAccrualDigest.NewSeasonalAccrualDigest("code-94", "name-598");
        Check.Equal(item.Status, SeasonalAccrualDigestStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalAccrualDigestMovesToApproved()
    {
        var item = SeasonalAccrualDigest.NewSeasonalAccrualDigest("code-94", "name-598");
        try
        {
            item.MoveTo(SeasonalAccrualDigestStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalAccrualDigestStatus.Approved, "status");
    }

    public static void SeasonalAccrualDigestRejectsMovesFromWithdrawn()
    {
        var item = SeasonalAccrualDigest.NewSeasonalAccrualDigest("code-94", "name-598");
        item.Status = SeasonalAccrualDigestStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalAccrualDigestStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalAccrualDigestStartsInProposed();
        SeasonalAccrualDigestMovesToApproved();
        SeasonalAccrualDigestRejectsMovesFromWithdrawn();
    }
}
