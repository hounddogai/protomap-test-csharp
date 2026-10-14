namespace Acme.Workforce.Engagement.Tests.Recognition.RecordItem;

using Acme.Workforce.Engagement.Recognition.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class BatchedRecognitionSlotStoreChecks
{
    public static void MemoryBatchedRecognitionSlotStoreFindsWhatItSaved()
    {
        var store = BatchedRecognitionSlotStore.NewMemoryBatchedRecognitionSlotStore();
        var item = BatchedRecognitionSlot.NewBatchedRecognitionSlot("code-87", "name-836");
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
        MemoryBatchedRecognitionSlotStoreFindsWhatItSaved();
    }
}
