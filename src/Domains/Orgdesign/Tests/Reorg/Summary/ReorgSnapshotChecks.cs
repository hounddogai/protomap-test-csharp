namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class ReorgSnapshotChecks
{
    public static void ReorgSnapshotStartsInNew()
    {
        var item = ReorgSnapshot.NewReorgSnapshot("code-169", "name-298");
        Check.Equal(item.Status, ReorgSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void ReorgSnapshotMovesToQueued()
    {
        var item = ReorgSnapshot.NewReorgSnapshot("code-169", "name-298");
        try
        {
            item.MoveTo(ReorgSnapshotStatus.Queued);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, ReorgSnapshotStatus.Queued, "status");
    }

    public static void ReorgSnapshotRejectsMovesFromFailed()
    {
        var item = ReorgSnapshot.NewReorgSnapshot("code-169", "name-298");
        item.Status = ReorgSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(ReorgSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        ReorgSnapshotStartsInNew();
        ReorgSnapshotMovesToQueued();
        ReorgSnapshotRejectsMovesFromFailed();
    }
}
