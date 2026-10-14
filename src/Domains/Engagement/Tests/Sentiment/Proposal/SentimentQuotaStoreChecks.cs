namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentQuotaStoreChecks
{
    public static void MemorySentimentQuotaStoreFindsWhatItSaved()
    {
        var store = SentimentQuotaStore.NewMemorySentimentQuotaStore();
        var item = SentimentQuota.NewSentimentQuota("code-492", "name-741");
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
        MemorySentimentQuotaStoreFindsWhatItSaved();
    }
}
