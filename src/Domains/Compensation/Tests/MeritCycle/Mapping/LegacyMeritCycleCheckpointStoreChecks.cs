namespace Acme.Workforce.Compensation.Tests.MeritCycle.Mapping;

using Acme.Workforce.Compensation.MeritCycle.Mapping;

/// <summary>Checks of mapping code, run by the project's runner.</summary>
public static class LegacyMeritCycleCheckpointStoreChecks
{
    public static void MemoryLegacyMeritCycleCheckpointStoreFindsWhatItSaved()
    {
        var store = LegacyMeritCycleCheckpointStore.NewMemoryLegacyMeritCycleCheckpointStore();
        var item = LegacyMeritCycleCheckpoint.NewLegacyMeritCycleCheckpoint("code-105", "name-501");
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
        MemoryLegacyMeritCycleCheckpointStoreFindsWhatItSaved();
    }
}
