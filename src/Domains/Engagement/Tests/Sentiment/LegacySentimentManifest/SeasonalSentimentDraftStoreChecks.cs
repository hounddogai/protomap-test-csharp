namespace Acme.Workforce.Engagement.Tests.Sentiment.LegacySentimentManifest;

using Acme.Workforce.Engagement.Sentiment.LegacySentimentManifest;

/// <summary>Checks of legacy sentiment manifest code, run by the project's runner.</summary>
public static class SeasonalSentimentDraftStoreChecks
{
    public static void MemorySeasonalSentimentDraftStoreFindsWhatItSaved()
    {
        var store = SeasonalSentimentDraftStore.NewMemorySeasonalSentimentDraftStore();
        var item = SeasonalSentimentDraft.NewSeasonalSentimentDraft("code-747", "label-953");
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
        MemorySeasonalSentimentDraftStoreFindsWhatItSaved();
    }
}
