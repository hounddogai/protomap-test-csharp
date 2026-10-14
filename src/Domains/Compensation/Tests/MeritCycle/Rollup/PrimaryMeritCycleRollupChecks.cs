namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryMeritCycleRollupChecks
{
    public static void PrimaryMeritCycleRollupStartsInNew()
    {
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("code-787", "title-879");
        Check.Equal(item.Status, PrimaryMeritCycleRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PrimaryMeritCycleRollupMovesToFailed()
    {
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("code-787", "title-879");
        try
        {
            item.MoveTo(PrimaryMeritCycleRollupStatus.Failed);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PrimaryMeritCycleRollupStatus.Failed, "status");
    }

    public static void PrimaryMeritCycleRollupRejectsMovesFromFailed()
    {
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("code-787", "title-879");
        item.Status = PrimaryMeritCycleRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(PrimaryMeritCycleRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        PrimaryMeritCycleRollupStartsInNew();
        PrimaryMeritCycleRollupMovesToFailed();
        PrimaryMeritCycleRollupRejectsMovesFromFailed();
    }
}
