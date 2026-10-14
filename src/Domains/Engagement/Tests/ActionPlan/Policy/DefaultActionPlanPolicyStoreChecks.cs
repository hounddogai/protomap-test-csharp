namespace Acme.Workforce.Engagement.Tests.ActionPlan.Policy;

using Acme.Workforce.Engagement.ActionPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class DefaultActionPlanPolicyStoreChecks
{
    public static void MemoryDefaultActionPlanPolicyStoreFindsWhatItSaved()
    {
        var store = DefaultActionPlanPolicyStore.NewMemoryDefaultActionPlanPolicyStore();
        var item = DefaultActionPlanPolicy.NewDefaultActionPlanPolicy("code-938", "label-25");
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
        MemoryDefaultActionPlanPolicyStoreFindsWhatItSaved();
    }
}
