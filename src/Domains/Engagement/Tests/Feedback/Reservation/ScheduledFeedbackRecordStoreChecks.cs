namespace Acme.Workforce.Engagement.Tests.Feedback.Reservation;

using Acme.Workforce.Engagement.Feedback.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class ScheduledFeedbackRecordStoreChecks
{
    public static void MemoryScheduledFeedbackRecordStoreFindsWhatItSaved()
    {
        var store = ScheduledFeedbackRecordStore.NewMemoryScheduledFeedbackRecordStore();
        var item = ScheduledFeedbackRecord.NewScheduledFeedbackRecord("code-702", "label-142");
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
        MemoryScheduledFeedbackRecordStoreFindsWhatItSaved();
    }
}
