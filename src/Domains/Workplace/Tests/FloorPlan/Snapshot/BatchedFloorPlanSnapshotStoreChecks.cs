namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class BatchedFloorPlanSnapshotStoreChecks
{
    public static void MemoryBatchedFloorPlanSnapshotStoreFindsWhatItSaved()
    {
        var store = BatchedFloorPlanSnapshotStore.NewMemoryBatchedFloorPlanSnapshotStore();
        var item = BatchedFloorPlanSnapshot.NewBatchedFloorPlanSnapshot("code-244", "name-817");
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
        MemoryBatchedFloorPlanSnapshotStoreFindsWhatItSaved();
    }
}
