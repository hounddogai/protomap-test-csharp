namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentSummaryStoreChecks
{
    public static void MemorySentimentSummaryStoreFindsWhatItSaved()
    {
        var store = SentimentSummaryStore.NewMemorySentimentSummaryStore();
        var item = SentimentSummary.NewSentimentSummary("code-835", "title-698");
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
        MemorySentimentSummaryStoreFindsWhatItSaved();
    }
}
