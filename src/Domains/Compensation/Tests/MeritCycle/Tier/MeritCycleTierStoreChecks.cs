namespace Acme.Workforce.Compensation.Tests.MeritCycle.Tier;

using Acme.Workforce.Compensation.MeritCycle.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class MeritCycleTierStoreChecks
{
    public static void MemoryMeritCycleTierStoreFindsWhatItSaved()
    {
        var store = MeritCycleTierStore.NewMemoryMeritCycleTierStore();
        var item = MeritCycleTier.NewMeritCycleTier("code-456", "title-852");
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
        MemoryMeritCycleTierStoreFindsWhatItSaved();
    }
}
