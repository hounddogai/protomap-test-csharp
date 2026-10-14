namespace Acme.Workforce.Orgdesign.Tests.Reorg.Schedule;

using Acme.Workforce.Orgdesign.Reorg.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ReorgAllocationChecks
{
    public static void ReorgAllocationStartsInIdle()
    {
        var item = ReorgAllocation.NewReorgAllocation("code-961", "name-701");
        Check.Equal(item.Status, ReorgAllocationStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgAllocationMovesToDraining()
    {
        var item = ReorgAllocation.NewReorgAllocation("code-961", "name-701");
        try
        {
            item.MoveTo(ReorgAllocationStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgAllocationStatus.Draining, "status");
    }

    public static void ReorgAllocationRejectsMovesFromStopped()
    {
        var item = ReorgAllocation.NewReorgAllocation("code-961", "name-701");
        item.Status = ReorgAllocationStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgAllocationStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgAllocationStartsInIdle();
        ReorgAllocationMovesToDraining();
        ReorgAllocationRejectsMovesFromStopped();
    }
}
