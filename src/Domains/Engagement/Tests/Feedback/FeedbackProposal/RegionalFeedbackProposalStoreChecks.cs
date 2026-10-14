namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackProposal;

using Acme.Workforce.Engagement.Feedback.FeedbackProposal;

/// <summary>Checks of feedback proposal code, run by the project's runner.</summary>
public static class RegionalFeedbackProposalStoreChecks
{
    public static void MemoryRegionalFeedbackProposalStoreFindsWhatItSaved()
    {
        var store = RegionalFeedbackProposalStore.NewMemoryRegionalFeedbackProposalStore();
        var item = RegionalFeedbackProposal.NewRegionalFeedbackProposal("code-184", "label-33");
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
        MemoryRegionalFeedbackProposalStoreFindsWhatItSaved();
    }
}
