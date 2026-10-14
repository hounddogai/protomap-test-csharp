namespace Acme.Workforce.Engagement.Tests.Feedback.Notice;

using Acme.Workforce.Engagement.Feedback.Notice;

/// <summary>Checks of notice code, run by the project's runner.</summary>
public static class BatchedFeedbackNoticeStoreChecks
{
    public static void MemoryBatchedFeedbackNoticeStoreFindsWhatItSaved()
    {
        var store = BatchedFeedbackNoticeStore.NewMemoryBatchedFeedbackNoticeStore();
        var item = BatchedFeedbackNotice.NewBatchedFeedbackNotice("code-33", "label-412");
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
        MemoryBatchedFeedbackNoticeStoreFindsWhatItSaved();
    }
}
