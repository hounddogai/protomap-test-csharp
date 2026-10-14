namespace Acme.Workforce.Engagement.Tests.Feedback.Manifest;

using Acme.Workforce.Engagement.Feedback.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class FeedbackLedgerStoreChecks
{
    public static void MemoryFeedbackLedgerStoreFindsWhatItSaved()
    {
        var store = FeedbackLedgerStore.NewMemoryFeedbackLedgerStore();
        var item = FeedbackLedger.NewFeedbackLedger("code-560", "label-163");
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
        MemoryFeedbackLedgerStoreFindsWhatItSaved();
    }
}
