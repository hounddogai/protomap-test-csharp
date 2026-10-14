namespace Acme.Workforce.Engagement.Tests.Sentiment.Manifest;

using Acme.Workforce.Engagement.Sentiment.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class ScheduledSentimentTargetStoreChecks
{
    public static void MemoryScheduledSentimentTargetStoreFindsWhatItSaved()
    {
        var store = ScheduledSentimentTargetStore.NewMemoryScheduledSentimentTargetStore();
        var item = ScheduledSentimentTarget.NewScheduledSentimentTarget("code-371", "name-460");
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
        MemoryScheduledSentimentTargetStoreFindsWhatItSaved();
    }
}
