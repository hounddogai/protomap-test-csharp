namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class AdaptiveFeedbackSummaryStoreChecks
{
    public static void MemoryAdaptiveFeedbackSummaryStoreFindsWhatItSaved()
    {
        var store = AdaptiveFeedbackSummaryStore.NewMemoryAdaptiveFeedbackSummaryStore();
        var item = AdaptiveFeedbackSummary.NewAdaptiveFeedbackSummary("code-115", "title-965");
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
        MemoryAdaptiveFeedbackSummaryStoreFindsWhatItSaved();
    }
}
