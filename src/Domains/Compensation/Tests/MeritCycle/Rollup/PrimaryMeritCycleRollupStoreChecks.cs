namespace Acme.Workforce.Compensation.Tests.MeritCycle.Rollup;

using Acme.Workforce.Compensation.MeritCycle.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class PrimaryMeritCycleRollupStoreChecks
{
    public static void MemoryPrimaryMeritCycleRollupStoreFindsWhatItSaved()
    {
        var store = PrimaryMeritCycleRollupStore.NewMemoryPrimaryMeritCycleRollupStore();
        var item = PrimaryMeritCycleRollup.NewPrimaryMeritCycleRollup("code-787", "title-879");
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
        MemoryPrimaryMeritCycleRollupStoreFindsWhatItSaved();
    }
}
