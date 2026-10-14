namespace Acme.Workforce.Orgdesign.Tests.Reorg.Checkpoint;

using Acme.Workforce.Orgdesign.Reorg.Checkpoint;

/// <summary>Checks of checkpoint code, run by the project's runner.</summary>
public static class BatchedReorgCheckpointStoreChecks
{
    public static void MemoryBatchedReorgCheckpointStoreFindsWhatItSaved()
    {
        var store = BatchedReorgCheckpointStore.NewMemoryBatchedReorgCheckpointStore();
        var item = BatchedReorgCheckpoint.NewBatchedReorgCheckpoint("code-172", "title-821");
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
        MemoryBatchedReorgCheckpointStoreFindsWhatItSaved();
    }
}
