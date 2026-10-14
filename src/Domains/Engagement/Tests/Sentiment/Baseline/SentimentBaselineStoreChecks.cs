namespace Acme.Workforce.Engagement.Tests.Sentiment.Baseline;

using Acme.Workforce.Engagement.Sentiment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class SentimentBaselineStoreChecks
{
    public static void MemorySentimentBaselineStoreFindsWhatItSaved()
    {
        var store = SentimentBaselineStore.NewMemorySentimentBaselineStore();
        var item = SentimentBaseline.NewSentimentBaseline("code-408", "label-691");
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
        MemorySentimentBaselineStoreFindsWhatItSaved();
    }
}
