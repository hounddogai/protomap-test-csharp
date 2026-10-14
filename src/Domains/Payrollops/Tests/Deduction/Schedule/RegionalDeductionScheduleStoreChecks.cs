namespace Acme.Workforce.Payrollops.Tests.Deduction.Schedule;

using Acme.Workforce.Payrollops.Deduction.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class RegionalDeductionScheduleStoreChecks
{
    public static void MemoryRegionalDeductionScheduleStoreFindsWhatItSaved()
    {
        var store = RegionalDeductionScheduleStore.NewMemoryRegionalDeductionScheduleStore();
        var item = RegionalDeductionSchedule.NewRegionalDeductionSchedule("code-650", "label-800");
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
        MemoryRegionalDeductionScheduleStoreFindsWhatItSaved();
    }
}
