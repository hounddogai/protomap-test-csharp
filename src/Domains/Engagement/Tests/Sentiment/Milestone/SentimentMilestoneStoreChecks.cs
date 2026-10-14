namespace Acme.Workforce.Engagement.Tests.Sentiment.Milestone;

using Acme.Workforce.Engagement.Sentiment.Milestone;

/// <summary>Checks of milestone code, run by the project's runner.</summary>
public static class SentimentMilestoneStoreChecks
{
    public static void MemorySentimentMilestoneStoreFindsWhatItSaved()
    {
        var store = SentimentMilestoneStore.NewMemorySentimentMilestoneStore();
        var item = SentimentMilestone.NewSentimentMilestone("code-717", "label-154");
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
        MemorySentimentMilestoneStoreFindsWhatItSaved();
    }
}
