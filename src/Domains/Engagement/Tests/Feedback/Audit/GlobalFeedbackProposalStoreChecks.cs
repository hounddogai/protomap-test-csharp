namespace Acme.Workforce.Engagement.Tests.Feedback.Audit;

using Acme.Workforce.Engagement.Feedback.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class GlobalFeedbackProposalStoreChecks
{
    public static void MemoryGlobalFeedbackProposalStoreFindsWhatItSaved()
    {
        var store = GlobalFeedbackProposalStore.NewMemoryGlobalFeedbackProposalStore();
        var item = GlobalFeedbackProposal.NewGlobalFeedbackProposal("code-123", "name-198");
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
        MemoryGlobalFeedbackProposalStoreFindsWhatItSaved();
    }
}
