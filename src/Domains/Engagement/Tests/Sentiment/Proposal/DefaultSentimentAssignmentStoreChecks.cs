namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class DefaultSentimentAssignmentStoreChecks
{
    public static void MemoryDefaultSentimentAssignmentStoreFindsWhatItSaved()
    {
        var store = DefaultSentimentAssignmentStore.NewMemoryDefaultSentimentAssignmentStore();
        var item = DefaultSentimentAssignment.NewDefaultSentimentAssignment("code-819", "title-577");
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
        MemoryDefaultSentimentAssignmentStoreFindsWhatItSaved();
    }
}
