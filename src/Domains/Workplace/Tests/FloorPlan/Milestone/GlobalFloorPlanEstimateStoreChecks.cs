namespace Acme.Workforce.Workplace.Tests.FloorPlan.Milestone;

using Acme.Workforce.Workplace.FloorPlan.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class GlobalFloorPlanEstimateStoreChecks
{
    public static void MemoryGlobalFloorPlanEstimateStoreFindsWhatItSaved()
    {
        var store = GlobalFloorPlanEstimateStore.NewMemoryGlobalFloorPlanEstimateStore();
        var item = GlobalFloorPlanEstimate.NewGlobalFloorPlanEstimate("code-346", "label-983");
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
        MemoryGlobalFloorPlanEstimateStoreFindsWhatItSaved();
    }
}
