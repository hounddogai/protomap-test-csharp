namespace Acme.Workforce.Compensation.Tests.MeritCycle.OverrideItem;

using Acme.Workforce.Compensation.MeritCycle.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class MeritCycleCheckpointStoreChecks
{
    public static void MemoryMeritCycleCheckpointStoreFindsWhatItSaved()
    {
        var store = MeritCycleCheckpointStore.NewMemoryMeritCycleCheckpointStore();
        var item = MeritCycleCheckpoint.NewMeritCycleCheckpoint("code-607", "name-46");
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
        MemoryMeritCycleCheckpointStoreFindsWhatItSaved();
    }
}
