namespace Acme.Workforce.Engagement.Tests.TownHall.Entry;

using Acme.Workforce.Engagement.TownHall.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class GlobalTownHallCheckpointChecks
{
    public static void GlobalTownHallCheckpointStartsInPlanned()
    {
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("code-933", "label-190");
        Check.Equal(item.Status, GlobalTownHallCheckpointStatus.Planned, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void GlobalTownHallCheckpointMovesToInProgress()
    {
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("code-933", "label-190");
        try
        {
            item.MoveTo(GlobalTownHallCheckpointStatus.InProgress);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, GlobalTownHallCheckpointStatus.InProgress, "status");
    }

    public static void GlobalTownHallCheckpointRejectsMovesFromCancelled()
    {
        var item = GlobalTownHallCheckpoint.NewGlobalTownHallCheckpoint("code-933", "label-190");
        item.Status = GlobalTownHallCheckpointStatus.Cancelled;
        Check.Fails(() => item.MoveTo(GlobalTownHallCheckpointStatus.Planned), "move");
    }

    public static void RunAll()
    {
        GlobalTownHallCheckpointStartsInPlanned();
        GlobalTownHallCheckpointMovesToInProgress();
        GlobalTownHallCheckpointRejectsMovesFromCancelled();
    }
}
