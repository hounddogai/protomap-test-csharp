namespace Acme.Workforce.Workplace.Tests.FloorPlan.PlanEntry;

using Acme.Workforce.Workplace.FloorPlan.PlanEntry;

/// <summary>Checks of plan entry code, run by the project's runner.</summary>
public static class SeasonalFloorPlanTierStoreChecks
{
    public static void MemorySeasonalFloorPlanTierStoreFindsWhatItSaved()
    {
        var store = SeasonalFloorPlanTierStore.NewMemorySeasonalFloorPlanTierStore();
        var item = SeasonalFloorPlanTier.NewSeasonalFloorPlanTier("code-88", "title-348");
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
        MemorySeasonalFloorPlanTierStoreFindsWhatItSaved();
    }
}
