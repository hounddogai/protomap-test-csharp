namespace Acme.Workforce.Engagement.Tests.Feedback.Rule;

using Acme.Workforce.Engagement.Feedback.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class FeedbackAssignmentStoreChecks
{
    public static void MemoryFeedbackAssignmentStoreFindsWhatItSaved()
    {
        var store = FeedbackAssignmentStore.NewMemoryFeedbackAssignmentStore();
        var item = FeedbackAssignment.NewFeedbackAssignment("code-743", "name-690");
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
        MemoryFeedbackAssignmentStoreFindsWhatItSaved();
    }
}
