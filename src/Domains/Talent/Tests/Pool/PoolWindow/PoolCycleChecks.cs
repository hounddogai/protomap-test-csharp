namespace Acme.Workforce.Talent.Tests.Pool.PoolWindow;

using Acme.Workforce.Talent.Pool.PoolWindow;

/// <summary>Checks of pool window code, run by the project's runner.</summary>
public static class PoolCycleChecks
{
    public static void PoolCycleStartsInPlanned()
    {
        var item = PoolCycle.NewPoolCycle("code-481", "title-642");
        Check.Equal(item.Status, PoolCycleStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PoolCycleMovesToScheduled()
    {
        var item = PoolCycle.NewPoolCycle("code-481", "title-642");
        try
        {
            item.MoveTo(PoolCycleStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PoolCycleStatus.Scheduled, "status");
    }

    public static void PoolCycleRejectsMovesFromCancelled()
    {
        var item = PoolCycle.NewPoolCycle("code-481", "title-642");
        item.Status = PoolCycleStatus.Cancelled;
        Check.Fails(() => item.MoveTo(PoolCycleStatus.Planned), "move");
    }

    public static void RunAll()
    {
        PoolCycleStartsInPlanned();
        PoolCycleMovesToScheduled();
        PoolCycleRejectsMovesFromCancelled();
    }
}
