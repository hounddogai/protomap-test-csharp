namespace Acme.Workforce.Talent.Tests.Mobility.Window;

using Acme.Workforce.Talent.Mobility.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class MobilityCheckpointStoreChecks
{
    public static void MemoryMobilityCheckpointStoreFindsWhatItSaved()
    {
        var store = MobilityCheckpointStore.NewMemoryMobilityCheckpointStore();
        var item = MobilityCheckpoint.NewMobilityCheckpoint("code-150", "name-773");
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
        MemoryMobilityCheckpointStoreFindsWhatItSaved();
    }
}
