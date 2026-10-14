namespace Acme.Workforce.Orgdesign.Tests.Reorg.Exception;

using Acme.Workforce.Orgdesign.Reorg.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class ReorgCheckpointStoreChecks
{
    public static void MemoryReorgCheckpointStoreFindsWhatItSaved()
    {
        var store = ReorgCheckpointStore.NewMemoryReorgCheckpointStore();
        var item = ReorgCheckpoint.NewReorgCheckpoint("code-880", "name-406");
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
        MemoryReorgCheckpointStoreFindsWhatItSaved();
    }
}
