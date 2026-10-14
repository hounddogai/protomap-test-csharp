namespace Acme.Workforce.Engagement.Tests.Feedback.SeasonalFeedbackReservation;

using Acme.Workforce.Engagement.Feedback.SeasonalFeedbackReservation;

/// <summary>Checks of seasonal feedback reservation code, run by the project's runner.</summary>
public static class StrictFeedbackReservationStoreChecks
{
    public static void MemoryStrictFeedbackReservationStoreFindsWhatItSaved()
    {
        var store = StrictFeedbackReservationStore.NewMemoryStrictFeedbackReservationStore();
        var item = StrictFeedbackReservation.NewStrictFeedbackReservation("code-295", "label-661");
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
        MemoryStrictFeedbackReservationStoreFindsWhatItSaved();
    }
}
