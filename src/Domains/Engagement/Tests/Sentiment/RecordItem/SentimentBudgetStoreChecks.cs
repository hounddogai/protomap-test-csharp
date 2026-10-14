namespace Acme.Workforce.Engagement.Tests.Sentiment.RecordItem;

using Acme.Workforce.Engagement.Sentiment.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class SentimentBudgetStoreChecks
{
    public static void MemorySentimentBudgetStoreFindsWhatItSaved()
    {
        var store = SentimentBudgetStore.NewMemorySentimentBudgetStore();
        var item = SentimentBudget.NewSentimentBudget("code-162", "label-150");
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
        MemorySentimentBudgetStoreFindsWhatItSaved();
    }
}
