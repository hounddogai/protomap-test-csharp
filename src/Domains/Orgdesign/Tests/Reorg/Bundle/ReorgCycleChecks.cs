namespace Acme.Workforce.Orgdesign.Tests.Reorg.Bundle;

using Acme.Workforce.Orgdesign.Reorg.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class ReorgCycleChecks
{
    public static void ReorgCycleStartsInIdle()
    {
        var item = ReorgCycle.NewReorgCycle("code-338", "label-539");
        Check.Equal(item.Status, ReorgCycleStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgCycleMovesToWarming()
    {
        var item = ReorgCycle.NewReorgCycle("code-338", "label-539");
        try
        {
            item.MoveTo(ReorgCycleStatus.Warming);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgCycleStatus.Warming, "status");
    }

    public static void ReorgCycleRejectsMovesFromStopped()
    {
        var item = ReorgCycle.NewReorgCycle("code-338", "label-539");
        item.Status = ReorgCycleStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgCycleStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgCycleStartsInIdle();
        ReorgCycleMovesToWarming();
        ReorgCycleRejectsMovesFromStopped();
    }
}
