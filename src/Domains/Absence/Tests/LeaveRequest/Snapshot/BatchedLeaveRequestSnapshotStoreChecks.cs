namespace Acme.Workforce.Absence.Tests.LeaveRequest.Snapshot;

using Acme.Workforce.Absence.LeaveRequest.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedLeaveRequestSnapshotStoreChecks
{
    public static void MemoryBatchedLeaveRequestSnapshotStoreFindsWhatItSaved()
    {
        var store = BatchedLeaveRequestSnapshotStore.NewMemoryBatchedLeaveRequestSnapshotStore();
        var item = BatchedLeaveRequestSnapshot.NewBatchedLeaveRequestSnapshot("code-390", "name-627");
        store.Save(item);
        if (store.Find(item.Code) is not { } found)
        {
            throw new InvalidOperationException("unexpected empty result");
        }
        Check.Equal(found.Code, item.Code, "code");
        Check.Equal(store.List().Count, 1, "count");
    }

    public static void RunAll()
    {
        MemoryBatchedLeaveRequestSnapshotStoreFindsWhatItSaved();
    }
}
