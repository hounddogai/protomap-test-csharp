namespace Acme.Workforce.Contractors.Tests.Engagement.Template;

using Acme.Workforce.Contractors.Engagement.Template;

/// <summary>Checks of template code, run by the project's runner.</summary>
public static class ManualEngagementRuleStoreChecks
{
    public static void MemoryManualEngagementRuleStoreFindsWhatItSaved()
    {
        var store = ManualEngagementRuleStore.NewMemoryManualEngagementRuleStore();
        var item = ManualEngagementRule.NewManualEngagementRule("code-974", "label-118");
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
        MemoryManualEngagementRuleStoreFindsWhatItSaved();
    }
}
