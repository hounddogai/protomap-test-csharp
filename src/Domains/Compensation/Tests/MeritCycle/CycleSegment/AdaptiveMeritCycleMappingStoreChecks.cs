namespace Acme.Workforce.Compensation.Tests.MeritCycle.CycleSegment;

using Acme.Workforce.Compensation.MeritCycle.CycleSegment;

/// <summary>Checks of cycle segment code, run by the project's runner.</summary>
public static class AdaptiveMeritCycleMappingStoreChecks
{
    public static void MemoryAdaptiveMeritCycleMappingStoreFindsWhatItSaved()
    {
        var store = AdaptiveMeritCycleMappingStore.NewMemoryAdaptiveMeritCycleMappingStore();
        var item = AdaptiveMeritCycleMapping.NewAdaptiveMeritCycleMapping("code-243", "name-690");
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
        MemoryAdaptiveMeritCycleMappingStoreFindsWhatItSaved();
    }
}
