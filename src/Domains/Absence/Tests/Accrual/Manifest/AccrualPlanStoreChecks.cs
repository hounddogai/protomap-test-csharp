namespace Acme.Workforce.Absence.Tests.Accrual.Manifest;

using Acme.Workforce.Absence.Accrual.Manifest;

/// <summary>Checks of manifest code, run by the project's runner.</summary>
public static class AccrualPlanStoreChecks
{
    public static void MemoryAccrualPlanStoreFindsWhatItSaved()
    {
        var store = AccrualPlanStore.NewMemoryAccrualPlanStore();
        var item = AccrualPlan.NewAccrualPlan("code-138", "title-827");
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
        MemoryAccrualPlanStoreFindsWhatItSaved();
    }
}
