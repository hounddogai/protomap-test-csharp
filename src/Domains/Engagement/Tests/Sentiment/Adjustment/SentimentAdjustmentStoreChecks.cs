namespace Acme.Workforce.Engagement.Tests.Sentiment.Adjustment;

using Acme.Workforce.Engagement.Sentiment.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class SentimentAdjustmentStoreChecks
{
    public static void MemorySentimentAdjustmentStoreFindsWhatItSaved()
    {
        var store = SentimentAdjustmentStore.NewMemorySentimentAdjustmentStore();
        var item = SentimentAdjustment.NewSentimentAdjustment("code-337", "title-94");
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
        MemorySentimentAdjustmentStoreFindsWhatItSaved();
    }
}
