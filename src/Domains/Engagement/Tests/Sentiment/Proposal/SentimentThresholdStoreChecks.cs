namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class SentimentThresholdStoreChecks
{
    public static void MemorySentimentThresholdStoreFindsWhatItSaved()
    {
        var store = SentimentThresholdStore.NewMemorySentimentThresholdStore();
        var item = SentimentThreshold.NewSentimentThreshold("code-661", "title-678");
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
        MemorySentimentThresholdStoreFindsWhatItSaved();
    }
}
