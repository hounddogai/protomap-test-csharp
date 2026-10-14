namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackReservation;

using Acme.Workforce.Engagement.Feedback.FeedbackReservation;

/// <summary>Checks of feedback reservation code, run by the project's runner.</summary>
public static class FallbackFeedbackReservationStoreChecks
{
    public static void MemoryFallbackFeedbackReservationStoreFindsWhatItSaved()
    {
        var store = FallbackFeedbackReservationStore.NewMemoryFallbackFeedbackReservationStore();
        var item = FallbackFeedbackReservation.NewFallbackFeedbackReservation("code-694", "label-960");
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
        MemoryFallbackFeedbackReservationStoreFindsWhatItSaved();
    }
}
