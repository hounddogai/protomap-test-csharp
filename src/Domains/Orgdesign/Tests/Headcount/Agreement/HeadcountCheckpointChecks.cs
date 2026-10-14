namespace Acme.Workforce.Orgdesign.Tests.Headcount.Agreement;

using Acme.Workforce.Orgdesign.Headcount.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class HeadcountCheckpointChecks
{
    public static void HeadcountCheckpointStartsInIdle()
    {
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("code-459", "name-34");
        Check.Equal(item.Status, HeadcountCheckpointStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void HeadcountCheckpointMovesToReady()
    {
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("code-459", "name-34");
        try
        {
            item.MoveTo(HeadcountCheckpointStatus.Ready);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, HeadcountCheckpointStatus.Ready, "status");
    }

    public static void HeadcountCheckpointRejectsMovesFromStopped()
    {
        var item = HeadcountCheckpoint.NewHeadcountCheckpoint("code-459", "name-34");
        item.Status = HeadcountCheckpointStatus.Stopped;
        Check.Fails(() => item.MoveTo(HeadcountCheckpointStatus.Idle), "move");
    }

    public static void RunAll()
    {
        HeadcountCheckpointStartsInIdle();
        HeadcountCheckpointMovesToReady();
        HeadcountCheckpointRejectsMovesFromStopped();
    }
}
