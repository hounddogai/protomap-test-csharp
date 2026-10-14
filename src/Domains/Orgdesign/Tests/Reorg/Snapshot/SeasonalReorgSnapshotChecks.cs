namespace Acme.Workforce.Orgdesign.Tests.Reorg.Snapshot;

using Acme.Workforce.Orgdesign.Reorg.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalReorgSnapshotChecks
{
    public static void SeasonalReorgSnapshotStartsInNew()
    {
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("code-478", "label-39");
        Check.Equal(item.Status, SeasonalReorgSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void SeasonalReorgSnapshotMovesToQueued()
    {
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("code-478", "label-39");
        try
        {
            item.MoveTo(SeasonalReorgSnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, SeasonalReorgSnapshotStatus.Queued, "status");
    }

    public static void SeasonalReorgSnapshotRejectsMovesFromFailed()
    {
        var item = SeasonalReorgSnapshot.NewSeasonalReorgSnapshot("code-478", "label-39");
        item.Status = SeasonalReorgSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(SeasonalReorgSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        SeasonalReorgSnapshotStartsInNew();
        SeasonalReorgSnapshotMovesToQueued();
        SeasonalReorgSnapshotRejectsMovesFromFailed();
    }
}
