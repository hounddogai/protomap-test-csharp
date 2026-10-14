namespace Acme.Workforce.Engagement.Tests.Sentiment.Proposal;

using Acme.Workforce.Engagement.Sentiment.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class ScheduledSentimentLedgerStoreChecks
{
    public static void MemoryScheduledSentimentLedgerStoreFindsWhatItSaved()
    {
        var store = ScheduledSentimentLedgerStore.NewMemoryScheduledSentimentLedgerStore();
        var item = ScheduledSentimentLedger.NewScheduledSentimentLedger("code-635", "label-225");
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
        MemoryScheduledSentimentLedgerStoreFindsWhatItSaved();
    }
}
