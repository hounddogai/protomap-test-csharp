namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class LenientSentimentQuotaStoreChecks
{
    public static void MemoryLenientSentimentQuotaStoreFindsWhatItSaved()
    {
        var store = LenientSentimentQuotaStore.NewMemoryLenientSentimentQuotaStore();
        var item = LenientSentimentQuota.NewLenientSentimentQuota("code-66", "title-774");
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
        MemoryLenientSentimentQuotaStoreFindsWhatItSaved();
    }
}
