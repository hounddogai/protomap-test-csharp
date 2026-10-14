namespace Acme.Workforce.Absence.Tests.ReturnPlan.Target;

using Acme.Workforce.Absence.ReturnPlan.Target;

/// <summary>Checks of target code, run by the project's runner.</summary>
public static class ReturnPlanCycleStoreChecks
{
    public static void MemoryReturnPlanCycleStoreFindsWhatItSaved()
    {
        var store = ReturnPlanCycleStore.NewMemoryReturnPlanCycleStore();
        var item = ReturnPlanCycle.NewReturnPlanCycle("code-170", "title-842");
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
        MemoryReturnPlanCycleStoreFindsWhatItSaved();
    }
}
