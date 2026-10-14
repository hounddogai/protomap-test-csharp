namespace Acme.Workforce.Engagement.Tests.Feedback.Proposal;

using Acme.Workforce.Engagement.Feedback.Proposal;

/// <summary>Checks of proposal code, run by the project's runner.</summary>
public static class FeedbackMilestoneStoreChecks
{
    public static void MemoryFeedbackMilestoneStoreFindsWhatItSaved()
    {
        var store = FeedbackMilestoneStore.NewMemoryFeedbackMilestoneStore();
        var item = FeedbackMilestone.NewFeedbackMilestone("code-416", "title-321");
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
        MemoryFeedbackMilestoneStoreFindsWhatItSaved();
    }
}
