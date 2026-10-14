namespace Acme.Workforce.Compensation.Tests.MeritCycle.Claim;

using Acme.Workforce.Compensation.MeritCycle.Claim;

/// <summary>Checks of claim code, run by the project's runner.</summary>
public static class GlobalMeritCycleRollupChecks
{
    public static void GlobalMeritCycleRollupStartsInNew()
    {
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("code-244", "name-185");
        Check.Equal(item.Status, GlobalMeritCycleRollupStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalMeritCycleRollupMovesToRunning()
    {
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("code-244", "name-185");
        try
        {
            item.MoveTo(GlobalMeritCycleRollupStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalMeritCycleRollupStatus.Running, "status");
    }

    public static void GlobalMeritCycleRollupRejectsMovesFromFailed()
    {
        var item = GlobalMeritCycleRollup.NewGlobalMeritCycleRollup("code-244", "name-185");
        item.Status = GlobalMeritCycleRollupStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalMeritCycleRollupStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalMeritCycleRollupStartsInNew();
        GlobalMeritCycleRollupMovesToRunning();
        GlobalMeritCycleRollupRejectsMovesFromFailed();
    }
}
