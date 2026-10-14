namespace Acme.Workforce.Engagement.Tests.Sentiment.Summary;

using Acme.Workforce.Engagement.Sentiment.Summary;

/// <summary>Checks of summary code, run by the project's runner.</summary>
public static class SentimentRuleStoreChecks
{
    public static void MemorySentimentRuleStoreFindsWhatItSaved()
    {
        var store = SentimentRuleStore.NewMemorySentimentRuleStore();
        var item = SentimentRule.NewSentimentRule("code-271", "name-872");
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
        MemorySentimentRuleStoreFindsWhatItSaved();
    }
}
