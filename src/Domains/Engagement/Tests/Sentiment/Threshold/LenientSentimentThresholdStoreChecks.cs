namespace Acme.Workforce.Engagement.Tests.Sentiment.Threshold;

using Acme.Workforce.Engagement.Sentiment.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class LenientSentimentThresholdStoreChecks
{
    public static void MemoryLenientSentimentThresholdStoreFindsWhatItSaved()
    {
        var store = LenientSentimentThresholdStore.NewMemoryLenientSentimentThresholdStore();
        var item = LenientSentimentThreshold.NewLenientSentimentThreshold("code-100", "label-929");
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
        MemoryLenientSentimentThresholdStoreFindsWhatItSaved();
    }
}
