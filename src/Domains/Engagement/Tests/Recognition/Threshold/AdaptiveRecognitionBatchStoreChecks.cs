namespace Acme.Workforce.Engagement.Tests.Recognition.Threshold;

using Acme.Workforce.Engagement.Recognition.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class AdaptiveRecognitionBatchStoreChecks
{
    public static void MemoryAdaptiveRecognitionBatchStoreFindsWhatItSaved()
    {
        var store = AdaptiveRecognitionBatchStore.NewMemoryAdaptiveRecognitionBatchStore();
        var item = AdaptiveRecognitionBatch.NewAdaptiveRecognitionBatch("code-203", "label-860");
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
        MemoryAdaptiveRecognitionBatchStoreFindsWhatItSaved();
    }
}
