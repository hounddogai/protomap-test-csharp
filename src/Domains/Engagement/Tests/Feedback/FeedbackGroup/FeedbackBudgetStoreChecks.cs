namespace Acme.Workforce.Engagement.Tests.Feedback.FeedbackGroup;

using Acme.Workforce.Engagement.Feedback.FeedbackGroup;

/// <summary>Checks of feedback group code, run by the project's runner.</summary>
public static class FeedbackBudgetStoreChecks
{
    public static void MemoryFeedbackBudgetStoreFindsWhatItSaved()
    {
        var store = FeedbackBudgetStore.NewMemoryFeedbackBudgetStore();
        var item = FeedbackBudget.NewFeedbackBudget("code-586", "label-382");
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
        MemoryFeedbackBudgetStoreFindsWhatItSaved();
    }
}
