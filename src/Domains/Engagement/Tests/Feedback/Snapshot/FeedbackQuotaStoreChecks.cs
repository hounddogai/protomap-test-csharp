namespace Acme.Workforce.Engagement.Tests.Feedback.Snapshot;

using Acme.Workforce.Engagement.Feedback.Snapshot;

/// <summary>Checks of snapshot code, run by the project's runner.</summary>
public static class FeedbackQuotaStoreChecks
{
    public static void MemoryFeedbackQuotaStoreFindsWhatItSaved()
    {
        var store = FeedbackQuotaStore.NewMemoryFeedbackQuotaStore();
        var item = FeedbackQuota.NewFeedbackQuota("code-86", "name-882");
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
        MemoryFeedbackQuotaStoreFindsWhatItSaved();
    }
}
