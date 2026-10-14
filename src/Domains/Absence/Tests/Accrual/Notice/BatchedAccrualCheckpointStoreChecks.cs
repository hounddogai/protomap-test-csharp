namespace Acme.Workforce.Absence.Tests.Accrual.Notice;

using Acme.Workforce.Absence.Accrual.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedAccrualCheckpointStoreChecks
{
    public static void MemoryBatchedAccrualCheckpointStoreFindsWhatItSaved()
    {
        var store = BatchedAccrualCheckpointStore.NewMemoryBatchedAccrualCheckpointStore();
        var item = BatchedAccrualCheckpoint.NewBatchedAccrualCheckpoint("code-107", "label-375");
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
        MemoryBatchedAccrualCheckpointStoreFindsWhatItSaved();
    }
}
