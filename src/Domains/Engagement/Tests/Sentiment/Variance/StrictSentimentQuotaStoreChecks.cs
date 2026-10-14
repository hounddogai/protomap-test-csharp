namespace Acme.Workforce.Engagement.Tests.Sentiment.Variance;

using Acme.Workforce.Engagement.Sentiment.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class StrictSentimentQuotaStoreChecks
{
    public static void MemoryStrictSentimentQuotaStoreFindsWhatItSaved()
    {
        var store = StrictSentimentQuotaStore.NewMemoryStrictSentimentQuotaStore();
        var item = StrictSentimentQuota.NewStrictSentimentQuota("code-661", "label-631");
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
        MemoryStrictSentimentQuotaStoreFindsWhatItSaved();
    }
}
