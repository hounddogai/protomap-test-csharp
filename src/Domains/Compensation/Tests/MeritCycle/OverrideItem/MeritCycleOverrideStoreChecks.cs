namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleOverrideStoreChecks
{
    public static void MemoryMeritCycleOverrideStoreFindsWhatItSaved()
    {
        var store = MeritCycleOverrideStore.NewMemoryMeritCycleOverrideStore();
        var item = MeritCycleOverride.NewMeritCycleOverride("code-517", "label-833");
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
        MemoryMeritCycleOverrideStoreFindsWhatItSaved();
    }
}
