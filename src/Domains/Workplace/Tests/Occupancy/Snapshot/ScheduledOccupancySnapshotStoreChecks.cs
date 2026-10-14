namespace Acme.Workforce.Workplace.Tests.Occupancy.Snapshot;

using Acme.Workforce.Workplace.Occupancy.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class ScheduledOccupancySnapshotStoreChecks
{
    public static void MemoryScheduledOccupancySnapshotStoreFindsWhatItSaved()
    {
        var store = ScheduledOccupancySnapshotStore.NewMemoryScheduledOccupancySnapshotStore();
        var item = ScheduledOccupancySnapshot.NewScheduledOccupancySnapshot("code-530", "name-431");
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
        MemoryScheduledOccupancySnapshotStoreFindsWhatItSaved();
    }
}
