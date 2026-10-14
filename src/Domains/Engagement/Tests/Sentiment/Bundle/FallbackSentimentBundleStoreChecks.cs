namespace Acme.Workforce.Engagement.Tests.Sentiment.Bundle;

using Acme.Workforce.Engagement.Sentiment.Bundle;

/// <summary>Checks of bundle code, run by the project's runner.</summary>
public static class FallbackSentimentBundleStoreChecks
{
    public static void MemoryFallbackSentimentBundleStoreFindsWhatItSaved()
    {
        var store = FallbackSentimentBundleStore.NewMemoryFallbackSentimentBundleStore();
        var item = FallbackSentimentBundle.NewFallbackSentimentBundle("code-293", "title-789");
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
        MemoryFallbackSentimentBundleStoreFindsWhatItSaved();
    }
}
