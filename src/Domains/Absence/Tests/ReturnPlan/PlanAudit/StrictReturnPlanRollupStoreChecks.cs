namespace Acme.Workforce.Absence.Tests.ReturnPlan.PlanAudit;

using Acme.Workforce.Absence.ReturnPlan.PlanAudit;

/// <summary>Checks of plan audit code, run by the project's runner.</summary>
public static class StrictReturnPlanRollupStoreChecks
{
    public static void MemoryStrictReturnPlanRollupStoreFindsWhatItSaved()
    {
        var store = StrictReturnPlanRollupStore.NewMemoryStrictReturnPlanRollupStore();
        var item = StrictReturnPlanRollup.NewStrictReturnPlanRollup("code-87", "title-623");
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
        MemoryStrictReturnPlanRollupStoreFindsWhatItSaved();
    }
}
