namespace Acme.Workforce.Orgdesign.Tests.Headcount.Slot;

using Acme.Workforce.Orgdesign.Headcount.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class GlobalHeadcountCheckpointChecks
{
    public static void GlobalHeadcountCheckpointStartsInOpen()
    {
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("code-385", "title-243");
        Check.Equal(item.Status, GlobalHeadcountCheckpointStatus.Open, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalHeadcountCheckpointMovesToReopened()
    {
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("code-385", "title-243");
        try
        {
            item.MoveTo(GlobalHeadcountCheckpointStatus.Reopened);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalHeadcountCheckpointStatus.Reopened, "status");
    }

    public static void GlobalHeadcountCheckpointRejectsMovesFromArchived()
    {
        var item = GlobalHeadcountCheckpoint.NewGlobalHeadcountCheckpoint("code-385", "title-243");
        item.Status = GlobalHeadcountCheckpointStatus.Archived;
        Check.Fails(() => item.MoveTo(GlobalHeadcountCheckpointStatus.Open), "move");
    }

    public static void RunAll()
    {
        GlobalHeadcountCheckpointStartsInOpen();
        GlobalHeadcountCheckpointMovesToReopened();
        GlobalHeadcountCheckpointRejectsMovesFromArchived();
    }
}
