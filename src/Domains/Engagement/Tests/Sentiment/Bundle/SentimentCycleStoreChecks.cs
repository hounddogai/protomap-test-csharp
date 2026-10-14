namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class SentimentCycleStoreChecks
{
    public static void MemorySentimentCycleStoreFindsWhatItSaved()
    {
        var store = SentimentCycleStore.NewMemorySentimentCycleStore();
        var item = SentimentCycle.NewSentimentCycle("code-799", "name-898");
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
        MemorySentimentCycleStoreFindsWhatItSaved();
    }
}
