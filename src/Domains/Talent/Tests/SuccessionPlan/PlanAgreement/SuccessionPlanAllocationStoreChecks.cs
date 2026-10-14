namespace Acme.Workforce.Talent.Tests.SuccessionPlan.PlanAgreement;

using Acme.Workforce.Talent.SuccessionPlan.PlanAgreement;

/// <summary>Checks of plan agreement code, run by the project's runner.</summary>
public static class SuccessionPlanAllocationStoreChecks
{
    public static void MemorySuccessionPlanAllocationStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanAllocationStore.NewMemorySuccessionPlanAllocationStore();
        var item = SuccessionPlanAllocation.NewSuccessionPlanAllocation("code-93", "label-548");
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
        MemorySuccessionPlanAllocationStoreFindsWhatItSaved();
    }
}
