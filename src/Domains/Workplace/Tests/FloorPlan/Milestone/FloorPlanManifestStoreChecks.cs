namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class FloorPlanManifestStoreChecks
{
    public static void MemoryFloorPlanManifestStoreFindsWhatItSaved()
    {
        var store = FloorPlanManifestStore.NewMemoryFloorPlanManifestStore();
        var item = FloorPlanManifest.NewFloorPlanManifest("code-20", "title-279");
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
        MemoryFloorPlanManifestStoreFindsWhatItSaved();
    }
}
