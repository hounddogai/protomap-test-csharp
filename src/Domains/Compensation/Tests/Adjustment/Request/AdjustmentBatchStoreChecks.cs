namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class AdjustmentBatchStoreChecks
{
    public static void MemoryAdjustmentBatchStoreFindsWhatItSaved()
    {
        var store = AdjustmentBatchStore.NewMemoryAdjustmentBatchStore();
        var item = AdjustmentBatch.NewAdjustmentBatch("code-467", "label-64");
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
        MemoryAdjustmentBatchStoreFindsWhatItSaved();
    }
}
