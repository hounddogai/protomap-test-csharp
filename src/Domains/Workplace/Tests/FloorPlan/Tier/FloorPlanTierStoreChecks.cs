namespace Acme.Workforce.Workplace.Tests.FloorPlan.Tier;

using Acme.Workforce.Workplace.FloorPlan.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class FloorPlanTierStoreChecks
{
    public static void MemoryFloorPlanTierStoreFindsWhatItSaved()
    {
        var store = FloorPlanTierStore.NewMemoryFloorPlanTierStore();
        var item = FloorPlanTier.NewFloorPlanTier("code-200", "name-625");
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
        MemoryFloorPlanTierStoreFindsWhatItSaved();
    }
}
