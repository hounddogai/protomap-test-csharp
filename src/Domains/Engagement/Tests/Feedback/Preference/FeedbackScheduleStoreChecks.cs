namespace Acme.Workforce.Engagement.Tests.Feedback.Preference;

using Acme.Workforce.Engagement.Feedback.Preference;

/// <summary>Checks of preference code, run by the project's runner.</summary>
public static class FeedbackScheduleStoreChecks
{
    public static void MemoryFeedbackScheduleStoreFindsWhatItSaved()
    {
        var store = FeedbackScheduleStore.NewMemoryFeedbackScheduleStore();
        var item = FeedbackSchedule.NewFeedbackSchedule("code-352", "label-613");
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
        MemoryFeedbackScheduleStoreFindsWhatItSaved();
    }
}
