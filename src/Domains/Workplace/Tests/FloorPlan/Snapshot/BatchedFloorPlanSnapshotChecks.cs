namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedFloorPlanSnapshotChecks
{
    public static void BatchedFloorPlanSnapshotStartsInProposed()
    {
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("code-244", "name-817");
        Check.Equal(item.Status, BatchedFloorPlanSnapshotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedFloorPlanSnapshotMovesToRejected()
    {
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("code-244", "name-817");
        try
        {
            item.MoveTo(BatchedFloorPlanSnapshotStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedFloorPlanSnapshotStatus.Rejected, "status");
    }

    public static void BatchedFloorPlanSnapshotRejectsMovesFromWithdrawn()
    {
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("code-244", "name-817");
        item.Status = BatchedFloorPlanSnapshotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedFloorPlanSnapshotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedFloorPlanSnapshotStartsInProposed();
        BatchedFloorPlanSnapshotMovesToRejected();
        BatchedFloorPlanSnapshotRejectsMovesFromWithdrawn();
    }
}
