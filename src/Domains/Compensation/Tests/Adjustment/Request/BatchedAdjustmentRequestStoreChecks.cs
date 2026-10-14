namespace Acme.Workforce.Compensation.Tests.Adjustment.Request;

using Acme.Workforce.Compensation.Adjustment.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class BatchedAdjustmentRequestStoreChecks
{
    public static void MemoryBatchedAdjustmentRequestStoreFindsWhatItSaved()
    {
        var store = BatchedAdjustmentRequestStore.NewMemoryBatchedAdjustmentRequestStore();
        var item = BatchedAdjustmentRequest.NewBatchedAdjustmentRequest("code-109", "title-32");
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
        MemoryBatchedAdjustmentRequestStoreFindsWhatItSaved();
    }
}
