namespace Acme.Workforce.Payrollops.Tests.Deduction.Cycle;

using Acme.Workforce.Payrollops.Deduction.Cycle;

/// <summary>Checks of cycle code, run by the project's runner.</summary>
public static class DeductionCycleStoreChecks
{
    public static void MemoryDeductionCycleStoreFindsWhatItSaved()
    {
        var store = DeductionCycleStore.NewMemoryDeductionCycleStore();
        var item = DeductionCycle.NewDeductionCycle("code-764", "name-510");
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
        MemoryDeductionCycleStoreFindsWhatItSaved();
    }
}
