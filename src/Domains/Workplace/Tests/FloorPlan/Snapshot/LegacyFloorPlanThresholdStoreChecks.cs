namespace Acme.Workforce.Workplace.Tests.FloorPlan.Snapshot;

using Acme.Workforce.Workplace.FloorPlan.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class LegacyFloorPlanThresholdStoreChecks
{
    public static void MemoryLegacyFloorPlanThresholdStoreFindsWhatItSaved()
    {
        var store = LegacyFloorPlanThresholdStore.NewMemoryLegacyFloorPlanThresholdStore();
        var item = LegacyFloorPlanThreshold.NewLegacyFloorPlanThreshold("code-663", "title-910");
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
        MemoryLegacyFloorPlanThresholdStoreFindsWhatItSaved();
    }
}
