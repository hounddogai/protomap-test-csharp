namespace Acme.Workforce.Talent.Tests.Pool.Variance;

using Acme.Workforce.Talent.Pool.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class GlobalPoolCycleChecks
{
    public static void GlobalPoolCycleStartsInNew()
    {
        var item = GlobalPoolCycle.NewGlobalPoolCycle("code-549", "name-411");
        Check.Equal(item.Status, GlobalPoolCycleStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalPoolCycleMovesToQueued()
    {
        var item = GlobalPoolCycle.NewGlobalPoolCycle("code-549", "name-411");
        try
        {
            item.MoveTo(GlobalPoolCycleStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalPoolCycleStatus.Queued, "status");
    }

    public static void GlobalPoolCycleRejectsMovesFromFailed()
    {
        var item = GlobalPoolCycle.NewGlobalPoolCycle("code-549", "name-411");
        item.Status = GlobalPoolCycleStatus.Failed;
        Check.Fails(() => item.MoveTo(GlobalPoolCycleStatus.New), "move");
    }

    public static void RunAll()
    {
        GlobalPoolCycleStartsInNew();
        GlobalPoolCycleMovesToQueued();
        GlobalPoolCycleRejectsMovesFromFailed();
    }
}
