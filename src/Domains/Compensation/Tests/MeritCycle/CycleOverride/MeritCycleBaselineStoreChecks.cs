namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleOverride;

using Acme.Workforce.Compensation.MeritCycle.CycleOverride;

/// <summary>Checks of cycle override code, run by the project's runner.</summary>
public static class MeritCycleBaselineStoreChecks
{
    public static void MemoryMeritCycleBaselineStoreFindsWhatItSaved()
    {
        var store = MeritCycleBaselineStore.NewMemoryMeritCycleBaselineStore();
        var item = MeritCycleBaseline.NewMeritCycleBaseline("code-972", "name-320");
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
        MemoryMeritCycleBaselineStoreFindsWhatItSaved();
    }
}
