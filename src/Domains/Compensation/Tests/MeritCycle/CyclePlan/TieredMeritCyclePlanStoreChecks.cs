namespace Acme.Workforce.Compensation.Tests.MeritCycle.CyclePlan;

using Acme.Workforce.Compensation.MeritCycle.CyclePlan;

/// <summary>Checks of cycle plan code, run by the project's runner.</summary>
public static class TieredMeritCyclePlanStoreChecks
{
    public static void MemoryTieredMeritCyclePlanStoreFindsWhatItSaved()
    {
        var store = TieredMeritCyclePlanStore.NewMemoryTieredMeritCyclePlanStore();
        var item = TieredMeritCyclePlan.NewTieredMeritCyclePlan("code-367", "name-402");
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
        MemoryTieredMeritCyclePlanStoreFindsWhatItSaved();
    }
}
