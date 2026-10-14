namespace Acme.Workforce.Engagement.Tests.TownHall.Threshold;

using Acme.Workforce.Engagement.TownHall.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LegacyTownHallCheckpointChecks
{
    public static void LegacyTownHallCheckpointStartsInNew()
    {
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("code-168", "label-406");
        Check.Equal(item.Status, LegacyTownHallCheckpointStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void LegacyTownHallCheckpointMovesToQueued()
    {
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("code-168", "label-406");
        try
        {
            item.MoveTo(LegacyTownHallCheckpointStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, LegacyTownHallCheckpointStatus.Queued, "status");
    }

    public static void LegacyTownHallCheckpointRejectsMovesFromFailed()
    {
        var item = LegacyTownHallCheckpoint.NewLegacyTownHallCheckpoint("code-168", "label-406");
        item.Status = LegacyTownHallCheckpointStatus.Failed;
        Check.Fails(() => item.MoveTo(LegacyTownHallCheckpointStatus.New), "move");
    }

    public static void RunAll()
    {
        LegacyTownHallCheckpointStartsInNew();
        LegacyTownHallCheckpointMovesToQueued();
        LegacyTownHallCheckpointRejectsMovesFromFailed();
    }
}
