namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleRollup;

using Acme.Workforce.Compensation.MeritCycle.CycleRollup;

/// <summary>Checks of cycle rollup code, run by the project's runner.</summary>
public static class SeasonalMeritCycleRollupChecks
{
    public static void SeasonalMeritCycleRollupStartsInProposed()
    {
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("code-324", "title-818");
        Check.Equal(item.Status, SeasonalMeritCycleRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalMeritCycleRollupMovesToApproved()
    {
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("code-324", "title-818");
        try
        {
            item.MoveTo(SeasonalMeritCycleRollupStatus.Approved);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalMeritCycleRollupStatus.Approved, "status");
    }

    public static void SeasonalMeritCycleRollupRejectsMovesFromWithdrawn()
    {
        var item = SeasonalMeritCycleRollup.NewSeasonalMeritCycleRollup("code-324", "title-818");
        item.Status = SeasonalMeritCycleRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalMeritCycleRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalMeritCycleRollupStartsInProposed();
        SeasonalMeritCycleRollupMovesToApproved();
        SeasonalMeritCycleRollupRejectsMovesFromWithdrawn();
    }
}
