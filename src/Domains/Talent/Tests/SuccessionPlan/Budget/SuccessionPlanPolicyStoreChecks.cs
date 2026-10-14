namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Budget;

using Acme.Workforce.Talent.SuccessionPlan.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class SuccessionPlanPolicyStoreChecks
{
    public static void MemorySuccessionPlanPolicyStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanPolicyStore.NewMemorySuccessionPlanPolicyStore();
        var item = SuccessionPlanPolicy.NewSuccessionPlanPolicy("code-915", "label-375");
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
        MemorySuccessionPlanPolicyStoreFindsWhatItSaved();
    }
}
