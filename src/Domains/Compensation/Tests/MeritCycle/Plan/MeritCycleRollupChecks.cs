namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleRollupChecks
{
    public static void MeritCycleRollupStartsInNew()
    {
        var item = MeritCycleRollup.NewMeritCycleRollup("code-717", "title-729");
        Check.Equal(item.Status, MeritCycleRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void MeritCycleRollupMovesToQueued()
    {
        var item = MeritCycleRollup.NewMeritCycleRollup("code-717", "title-729");
        try
        {
            item.MoveTo(MeritCycleRollupStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, MeritCycleRollupStatus.Queued, "status");
    }

    public static void MeritCycleRollupRejectsMovesFromFailed()
    {
        var item = MeritCycleRollup.NewMeritCycleRollup("code-717", "title-729");
        item.Status = MeritCycleRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(MeritCycleRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        MeritCycleRollupStartsInNew();
        MeritCycleRollupMovesToQueued();
        MeritCycleRollupRejectsMovesFromFailed();
    }
}
