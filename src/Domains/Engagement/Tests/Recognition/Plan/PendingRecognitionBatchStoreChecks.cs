namespace Acme.Workforce.Engagement.Tests.Recognition.Plan;

using Acme.Workforce.Engagement.Recognition.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class PendingRecognitionBatchStoreChecks
{
    public static void MemoryPendingRecognitionBatchStoreFindsWhatItSaved()
    {
        var store = PendingRecognitionBatchStore.NewMemoryPendingRecognitionBatchStore();
        var item = PendingRecognitionBatch.NewPendingRecognitionBatch("code-835", "title-732");
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
        MemoryPendingRecognitionBatchStoreFindsWhatItSaved();
    }
}
