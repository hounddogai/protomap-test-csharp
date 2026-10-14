namespace Acme.Workforce.Orgdesign.Tests.Reorg.ReorgSnapshot;

using Acme.Workforce.Orgdesign.Reorg.ReorgSnapshot;

/// <summary>Checks of reorg snapshot code, run by the project's runner.</summary>
public static class BatchedReorgSnapshotChecks
{
    public static void BatchedReorgSnapshotStartsInIdle()
    {
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("code-765", "name-413");
        Check.Equal(item.Status, BatchedReorgSnapshotStatus.Idle, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedReorgSnapshotMovesToDraining()
    {
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("code-765", "name-413");
        try
        {
            item.MoveTo(BatchedReorgSnapshotStatus.Draining);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedReorgSnapshotStatus.Draining, "status");
    }

    public static void BatchedReorgSnapshotRejectsMovesFromStopped()
    {
        var item = BatchedReorgSnapshot.NewBatchedReorgSnapshot("code-765", "name-413");
        item.Status = BatchedReorgSnapshotStatus.Stopped;
        Check.Fails(() => item.MoveTo(BatchedReorgSnapshotStatus.Idle), "move");
    }

    public static void RunAll()
    {
        BatchedReorgSnapshotStartsInIdle();
        BatchedReorgSnapshotMovesToDraining();
        BatchedReorgSnapshotRejectsMovesFromStopped();
    }
}
