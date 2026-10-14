namespace Acme.Workforce.Engagement.Tests.Feedback.Quota;

using Acme.Workforce.Engagement.Feedback.Quota;

/// <summary>Checks of quota code, run by the project's runner.</summary>
public static class StrictFeedbackQuotaStoreChecks
{
    public static void MemoryStrictFeedbackQuotaStoreFindsWhatItSaved()
    {
        var store = StrictFeedbackQuotaStore.NewMemoryStrictFeedbackQuotaStore();
        var item = StrictFeedbackQuota.NewStrictFeedbackQuota("code-716", "title-885");
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
        MemoryStrictFeedbackQuotaStoreFindsWhatItSaved();
    }
}
