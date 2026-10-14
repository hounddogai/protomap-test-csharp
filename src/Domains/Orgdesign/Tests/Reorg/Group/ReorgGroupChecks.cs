namespace Acme.Workforce.Orgdesign.Tests.Reorg.Group;

using Acme.Workforce.Orgdesign.Reorg.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class ReorgGroupChecks
{
    public static void ReorgGroupStartsInIdle()
    {
        var item = ReorgGroup.NewReorgGroup("code-128", "title-100");
        Check.Equal(item.Status, ReorgGroupStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgGroupMovesToDraining()
    {
        var item = ReorgGroup.NewReorgGroup("code-128", "title-100");
        try
        {
            item.MoveTo(ReorgGroupStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgGroupStatus.Draining, "status");
    }

    public static void ReorgGroupRejectsMovesFromStopped()
    {
        var item = ReorgGroup.NewReorgGroup("code-128", "title-100");
        item.Status = ReorgGroupStatus.Stopped;
        Check.Fails(() => item.MoveTo(ReorgGroupStatus.Idle), "move");
    }

    public static void RunAll()
    {
        ReorgGroupStartsInIdle();
        ReorgGroupMovesToDraining();
        ReorgGroupRejectsMovesFromStopped();
    }
}
