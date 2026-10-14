namespace Acme.Workforce.Engagement.Tests.ActionPlan.RecordItem;

using Acme.Workforce.Engagement.ActionPlan.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class ActionPlanRuleStoreChecks
{
    public static void MemoryActionPlanRuleStoreFindsWhatItSaved()
    {
        var store = ActionPlanRuleStore.NewMemoryActionPlanRuleStore();
        var item = ActionPlanRule.NewActionPlanRule("code-961", "name-670");
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
        MemoryActionPlanRuleStoreFindsWhatItSaved();
    }
}
