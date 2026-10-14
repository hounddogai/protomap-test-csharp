namespace Acme.Workforce.Absence.Tests.ReturnPlan.Policy;

using Acme.Workforce.Absence.ReturnPlan.Policy;

/// <summary>Checks of policy code, run by the project's runner.</summary>
public static class ReturnPlanPolicyStoreChecks
{
    public static void MemoryReturnPlanPolicyStoreFindsWhatItSaved()
    {
        var store = ReturnPlanPolicyStore.NewMemoryReturnPlanPolicyStore();
        var item = ReturnPlanPolicy.NewReturnPlanPolicy("code-937", "label-344");
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
        MemoryReturnPlanPolicyStoreFindsWhatItSaved();
    }
}
