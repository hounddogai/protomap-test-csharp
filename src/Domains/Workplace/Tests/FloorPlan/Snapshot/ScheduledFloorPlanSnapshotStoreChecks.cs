namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledFloorPlanSnapshotStoreChecks
{
    public static void MemoryScheduledFloorPlanSnapshotStoreFindsWhatItSaved()
    {
        var store = ScheduledFloorPlanSnapshotStore.NewMemoryScheduledFloorPlanSnapshotStore();
        var item = ScheduledFloorPlanSnapshot.NewScheduledFloorPlanSnapshot("code-704", "title-152");
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
        MemoryScheduledFloorPlanSnapshotStoreFindsWhatItSaved();
    }
}
