namespace Acme.Workforce.Workplace.Tests.FloorPlan.Threshold;

using Acme.Workforce.Workplace.FloorPlan.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class FloorPlanThresholdStoreChecks
{
    public static void MemoryFloorPlanThresholdStoreFindsWhatItSaved()
    {
        var store = FloorPlanThresholdStore.NewMemoryFloorPlanThresholdStore();
        var item = FloorPlanThreshold.NewFloorPlanThreshold("code-250", "title-77");
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
        MemoryFloorPlanThresholdStoreFindsWhatItSaved();
    }
}
