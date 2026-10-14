namespace Acme.Workforce.Talent.Tests.Pool.PoolDraft;

using Acme.Workforce.Talent.Pool.PoolDraft;

/// <summary>Checks of pool draft code, run by the project's runner.</summary>
public static class SeasonalPoolOverrideChecks
{
    public static void SeasonalPoolOverrideStartsInProposed()
    {
        var item = SeasonalPoolOverride.NewSeasonalPoolOverride("code-230", "title-903");
        Check.Equal(item.Status, SeasonalPoolOverrideStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalPoolOverrideMovesToApproved()
    {
        var item = SeasonalPoolOverride.NewSeasonalPoolOverride("code-230", "title-903");
        try
        {
            item.MoveTo(SeasonalPoolOverrideStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalPoolOverrideStatus.Approved, "status");
    }

    public static void SeasonalPoolOverrideRejectsMovesFromWithdrawn()
    {
        var item = SeasonalPoolOverride.NewSeasonalPoolOverride("code-230", "title-903");
        item.Status = SeasonalPoolOverrideStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalPoolOverrideStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalPoolOverrideStartsInProposed();
        SeasonalPoolOverrideMovesToApproved();
        SeasonalPoolOverrideRejectsMovesFromWithdrawn();
    }
}
