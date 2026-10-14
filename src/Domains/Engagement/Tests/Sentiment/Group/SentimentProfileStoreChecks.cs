namespace Acme.Workforce.Engagement.Tests.Sentiment.Group;

using Acme.Workforce.Engagement.Sentiment.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class SentimentProfileStoreChecks
{
    public static void MemorySentimentProfileStoreFindsWhatItSaved()
    {
        var store = SentimentProfileStore.NewMemorySentimentProfileStore();
        var item = SentimentProfile.NewSentimentProfile("code-989", "name-325");
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
        MemorySentimentProfileStoreFindsWhatItSaved();
    }
}
