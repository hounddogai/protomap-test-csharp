namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAuditStoreChecks
{
    public static void MemorySentimentAuditStoreFindsWhatItSaved()
    {
        var store = SentimentAuditStore.NewMemorySentimentAuditStore();
        var item = SentimentAudit.NewSentimentAudit("code-735", "label-38");
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
        MemorySentimentAuditStoreFindsWhatItSaved();
    }
}
