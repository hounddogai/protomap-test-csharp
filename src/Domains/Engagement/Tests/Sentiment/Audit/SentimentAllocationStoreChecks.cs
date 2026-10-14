namespace Acme.Workforce.Engagement.Tests.Sentiment.Audit;

using Acme.Workforce.Engagement.Sentiment.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class SentimentAllocationStoreChecks
{
    public static void MemorySentimentAllocationStoreFindsWhatItSaved()
    {
        var store = SentimentAllocationStore.NewMemorySentimentAllocationStore();
        var item = SentimentAllocation.NewSentimentAllocation("code-746", "name-550");
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
        MemorySentimentAllocationStoreFindsWhatItSaved();
    }
}
