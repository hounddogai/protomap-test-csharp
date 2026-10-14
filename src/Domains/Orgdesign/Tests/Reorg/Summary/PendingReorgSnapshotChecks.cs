namespace Acme.Workforce.Orgdesign.Tests.Reorg.Summary;

using Acme.Workforce.Orgdesign.Reorg.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class PendingReorgSnapshotChecks
{
    public static void PendingReorgSnapshotStartsInNew()
    {
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("code-788", "label-433");
        Check.Equal(item.Status, PendingReorgSnapshotStatus.New, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void PendingReorgSnapshotMovesToRunning()
    {
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("code-788", "label-433");
        try
        {
            item.MoveTo(PendingReorgSnapshotStatus.Running);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, PendingReorgSnapshotStatus.Running, "status");
    }

    public static void PendingReorgSnapshotRejectsMovesFromFailed()
    {
        var item = PendingReorgSnapshot.NewPendingReorgSnapshot("code-788", "label-433");
        item.Status = PendingReorgSnapshotStatus.Failed;
        Check.Fails(() => item.MoveTo(PendingReorgSnapshotStatus.New), "move");
    }

    public static void RunAll()
    {
        PendingReorgSnapshotStartsInNew();
        PendingReorgSnapshotMovesToRunning();
        PendingReorgSnapshotRejectsMovesFromFailed();
    }
}
