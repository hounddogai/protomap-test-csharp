namespace Acme.Workforce.Absence.Tests.Policy.Rule;

using Acme.Workforce.Absence.Policy.Rule;

/// <summary>Checks of rule code, run by the project's runner.</summary>
public static class PolicyPlanStoreChecks
{
    public static void MemoryPolicyPlanStoreFindsWhatItSaved()
    {
        var store = PolicyPlanStore.NewMemoryPolicyPlanStore();
        var item = PolicyPlan.NewPolicyPlan("code-514", "label-866");
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
        MemoryPolicyPlanStoreFindsWhatItSaved();
    }
}
