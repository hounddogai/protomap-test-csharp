namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedLeaveRequestSnapshotChecks
{
    public static void BatchedLeaveRequestSnapshotStartsInProposed()
    {
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("code-390", "name-627");
        Check.Equal(item.Status, BatchedLeaveRequestSnapshotStatus.Proposed, "status");
        Check.True(!item.IsTerminal(), "terminal");
    }

    public static void BatchedLeaveRequestSnapshotMovesToRejected()
    {
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("code-390", "name-627");
        try
        {
            item.MoveTo(BatchedLeaveRequestSnapshotStatus.Rejected);
        }
        catch (InvalidOperationException)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(item.Status, BatchedLeaveRequestSnapshotStatus.Rejected, "status");
    }

    public static void BatchedLeaveRequestSnapshotRejectsMovesFromWithdrawn()
    {
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("code-390", "name-627");
        item.Status = BatchedLeaveRequestSnapshotStatus.Withdrawn;
        Check.Fails(() => item.MoveTo(BatchedLeaveRequestSnapshotStatus.Proposed), "move");
    }

    public static void RunAll()
    {
        BatchedLeaveRequestSnapshotStartsInProposed();
        BatchedLeaveRequestSnapshotMovesToRejected();
        BatchedLeaveRequestSnapshotRejectsMovesFromWithdrawn();
    }
}
