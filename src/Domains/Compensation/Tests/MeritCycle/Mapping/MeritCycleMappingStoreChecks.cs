namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class MeritCycleMappingStoreChecks
{
    public static void MemoryMeritCycleMappingStoreFindsWhatItSaved()
    {
        var store = MeritCycleMappingStore.NewMemoryMeritCycleMappingStore();
        var item = MeritCycleMapping.NewMeritCycleMapping("code-48", "title-384");
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
        MemoryMeritCycleMappingStoreFindsWhatItSaved();
    }
}
