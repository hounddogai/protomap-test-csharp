namespace Acme.Workforce.Engagement.Tests.Feedback.Baseline;

using Acme.Workforce.Engagement.Feedback.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class ScheduledFeedbackBaselineStoreChecks
{
    public static void MemoryScheduledFeedbackBaselineStoreFindsWhatItSaved()
    {
        var store = ScheduledFeedbackBaselineStore.NewMemoryScheduledFeedbackBaselineStore();
        var item = ScheduledFeedbackBaseline.NewScheduledFeedbackBaseline("code-68", "label-810");
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
        MemoryScheduledFeedbackBaselineStoreFindsWhatItSaved();
    }
}
