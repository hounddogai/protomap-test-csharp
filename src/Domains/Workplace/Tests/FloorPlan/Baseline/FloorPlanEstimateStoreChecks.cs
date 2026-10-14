namespace Acme.Workforce.Workplace.Tests.FloorPlan.Baseline;

using Acme.Workforce.Workplace.FloorPlan.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class FloorPlanEstimateStoreChecks
{
    public static void MemoryFloorPlanEstimateStoreFindsWhatItSaved()
    {
        var store = FloorPlanEstimateStore.NewMemoryFloorPlanEstimateStore();
        var item = FloorPlanEstimate.NewFloorPlanEstimate("code-629", "label-954");
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
        MemoryFloorPlanEstimateStoreFindsWhatItSaved();
    }
}
