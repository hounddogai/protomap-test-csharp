namespace Acme.Workforce.Compensation.Tests.MeritCycle.Metric;

using Acme.Workforce.Compensation.MeritCycle.Metric;

/// <summary>Checks of metric code, run by the project's runner.</summary>
public static class TieredMeritCycleCheckpointStoreChecks
{
    public static void MemoryTieredMeritCycleCheckpointStoreFindsWhatItSaved()
    {
        var store = TieredMeritCycleCheckpointStore.NewMemoryTieredMeritCycleCheckpointStore();
        var item = TieredMeritCycleCheckpoint.NewTieredMeritCycleCheckpoint("code-661", "title-457");
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
        MemoryTieredMeritCycleCheckpointStoreFindsWhatItSaved();
    }
}
