namespace Acme.Workforce.Talent.Tests.SuccessionPlan.Agreement;

using Acme.Workforce.Talent.SuccessionPlan.Agreement;

/// <summary>Checks of agreement code, run by the project's runner.</summary>
public static class SuccessionPlanCycleStoreChecks
{
    public static void MemorySuccessionPlanCycleStoreFindsWhatItSaved()
    {
        var store = SuccessionPlanCycleStore.NewMemorySuccessionPlanCycleStore();
        var item = SuccessionPlanCycle.NewSuccessionPlanCycle("code-221", "label-18");
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
        MemorySuccessionPlanCycleStoreFindsWhatItSaved();
    }
}
