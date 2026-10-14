namespace Acme.Workforce.Workplace.Tests.FloorPlan.Item;

using Acme.Workforce.Workplace.FloorPlan.Item;

/// <summary>Checks of item code, run by the project's runner.</summary>
public static class FloorPlanSnapshotStoreChecks
{
    public static void MemoryFloorPlanSnapshotStoreFindsWhatItSaved()
    {
        var store = FloorPlanSnapshotStore.NewMemoryFloorPlanSnapshotStore();
        var item = FloorPlanSnapshot.NewFloorPlanSnapshot("code-506", "title-55");
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
        MemoryFloorPlanSnapshotStoreFindsWhatItSaved();
    }
}
