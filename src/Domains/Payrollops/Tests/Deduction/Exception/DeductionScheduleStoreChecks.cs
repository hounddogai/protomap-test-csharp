namespace Acme.Workforce.Payrollops.Tests.Deduction.Exception;

using Acme.Workforce.Payrollops.Deduction.Exception;

/// <summary>Checks of exception code, run by the project's runner.</summary>
public static class DeductionScheduleStoreChecks
{
    public static void MemoryDeductionScheduleStoreFindsWhatItSaved()
    {
        var store = DeductionScheduleStore.NewMemoryDeductionScheduleStore();
        var item = DeductionSchedule.NewDeductionSchedule("code-584", "title-743");
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
        MemoryDeductionScheduleStoreFindsWhatItSaved();
    }
}
