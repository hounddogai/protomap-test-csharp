namespace Acme.Workforce.Engagement.Tests.Feedback.Score;

using Acme.Workforce.Engagement.Feedback.Score;

/// <summary>Checks of score code, run by the project's runner.</summary>
public static class BatchedFeedbackScoreStoreChecks
{
    public static void MemoryBatchedFeedbackScoreStoreFindsWhatItSaved()
    {
        var store = BatchedFeedbackScoreStore.NewMemoryBatchedFeedbackScoreStore();
        var item = BatchedFeedbackScore.NewBatchedFeedbackScore("code-937", "title-217");
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
        MemoryBatchedFeedbackScoreStoreFindsWhatItSaved();
    }
}
