namespace Acme.Workforce.Engagement.Tests.Feedback.Slot;

using Acme.Workforce.Engagement.Feedback.Slot;

/// <summary>Checks of slot code, run by the project's runner.</summary>
public static class CappedFeedbackRuleStoreChecks
{
    public static void MemoryCappedFeedbackRuleStoreFindsWhatItSaved()
    {
        var store = CappedFeedbackRuleStore.NewMemoryCappedFeedbackRuleStore();
        var item = CappedFeedbackRule.NewCappedFeedbackRule("code-934", "label-501");
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
        MemoryCappedFeedbackRuleStoreFindsWhatItSaved();
    }
}
