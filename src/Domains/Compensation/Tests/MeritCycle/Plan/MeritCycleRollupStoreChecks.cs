namespace Acme.Workforce.Compensation.Tests.MeritCycle.Plan;

using Acme.Workforce.Compensation.MeritCycle.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class MeritCycleRollupStoreChecks
{
    public static void MemoryMeritCycleRollupStoreFindsWhatItSaved()
    {
        var store = MeritCycleRollupStore.NewMemoryMeritCycleRollupStore();
        var item = MeritCycleRollup.NewMeritCycleRollup("code-717", "title-729");
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
        MemoryMeritCycleRollupStoreFindsWhatItSaved();
    }
}
