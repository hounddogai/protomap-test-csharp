namespace Acme.Workforce.Absence.Tests.ReturnPlan.Entry;

using Acme.Workforce.Absence.ReturnPlan.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class SeasonalReturnPlanRollupChecks
{
    public static void SeasonalReturnPlanRollupStartsInProposed()
    {
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("code-318", "name-180");
        Check.Equal(item.Status, SeasonalReturnPlanRollupStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalReturnPlanRollupMovesToRejected()
    {
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("code-318", "name-180");
        try
        {
            item.MoveTo(SeasonalReturnPlanRollupStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalReturnPlanRollupStatus.Rejected, "status");
    }

    public static void SeasonalReturnPlanRollupRejectsMovesFromWithdrawn()
    {
        var item = SeasonalReturnPlanRollup.NewSeasonalReturnPlanRollup("code-318", "name-180");
        item.Status = SeasonalReturnPlanRollupStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(SeasonalReturnPlanRollupStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        SeasonalReturnPlanRollupStartsInProposed();
        SeasonalReturnPlanRollupMovesToRejected();
        SeasonalReturnPlanRollupRejectsMovesFromWithdrawn();
    }
}
