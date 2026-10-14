namespace Acme.Workforce.Orgdesign.Tests.Reorg.Metric;

using Acme.Workforce.Orgdesign.Reorg.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class ReorgMappingChecks
{
    public static void ReorgMappingStartsInIdle()
    {
        var item = ReorgMapping.NewReorgMapping("code-799", "name-176");
        Check.Equal(item.Status, ReorgMappingStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgMappingMovesToDraining()
    {
        var item = ReorgMapping.NewReorgMapping("code-799", "name-176");
        try
        {
            item.MoveTo(ReorgMappingStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgMappingStatus.Draining, "status");
    }

    public static void ReorgMappingRejectsMovesFromStopped()
    {
        var item = ReorgMapping.NewReorgMapping("code-799", "name-176");
        item.Status = ReorgMappingStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgMappingStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgMappingStartsInIdle();
        ReorgMappingMovesToDraining();
        ReorgMappingRejectsMovesFromStopped();
    }
}
