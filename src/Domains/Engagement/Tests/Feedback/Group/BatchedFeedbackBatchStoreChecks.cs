namespace Acme.Workforce.Engagement.Tests.Feedback.Group;

using Acme.Workforce.Engagement.Feedback.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BatchedFeedbackBatchStoreChecks
{
    public static void MemoryBatchedFeedbackBatchStoreFindsWhatItSaved()
    {
        var store = BatchedFeedbackBatchStore.NewMemoryBatchedFeedbackBatchStore();
        var item = BatchedFeedbackBatch.NewBatchedFeedbackBatch("code-771", "title-517");
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
        MemoryBatchedFeedbackBatchStoreFindsWhatItSaved();
    }
}
