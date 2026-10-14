namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class SeasonalFloorPlanBatchStoreChecks
{
    public static void MemorySeasonalFloorPlanBatchStoreFindsWhatItSaved()
    {
        var store = SeasonalFloorPlanBatchStore.NewMemorySeasonalFloorPlanBatchStore();
        var item = SeasonalFloorPlanBatch.NewSeasonalFloorPlanBatch("code-23", "name-335");
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
        MemorySeasonalFloorPlanBatchStoreFindsWhatItSaved();
    }
}
