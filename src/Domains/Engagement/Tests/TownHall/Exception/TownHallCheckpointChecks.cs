namespace Acme.Workforce.Engagement.Tests.TownHall.Exception;

using Acme.Workforce.Engagement.TownHall.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class TownHallCheckpointChecks
{
    public static void TownHallCheckpointStartsInPlanned()
    {
        var item = TownHallCheckpoint.NewTownHallCheckpoint("code-269", "title-25");
        Check.Equal(item.Status, TownHallCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void TownHallCheckpointMovesToScheduled()
    {
        var item = TownHallCheckpoint.NewTownHallCheckpoint("code-269", "title-25");
        try
        {
            item.MoveTo(TownHallCheckpointStatus.Scheduled);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, TownHallCheckpointStatus.Scheduled, "status");
    }

    public static void TownHallCheckpointRejectsMovesFromCancelled()
    {
        var item = TownHallCheckpoint.NewTownHallCheckpoint("code-269", "title-25");
        item.Status = TownHallCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(TownHallCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        TownHallCheckpointStartsInPlanned();
        TownHallCheckpointMovesToScheduled();
        TownHallCheckpointRejectsMovesFromCancelled();
    }
}
