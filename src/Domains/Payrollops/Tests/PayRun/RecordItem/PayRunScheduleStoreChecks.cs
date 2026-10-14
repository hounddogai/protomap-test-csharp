namespace Acme.Workforce.Payrollops.Tests.PayRun.RecordItem;

using Acme.Workforce.Payrollops.PayRun.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class PayRunScheduleStoreChecks
{
    public static void MemoryPayRunScheduleStoreFindsWhatItSaved()
    {
        var store = PayRunScheduleStore.NewMemoryPayRunScheduleStore();
        var item = PayRunSchedule.NewPayRunSchedule("code-555", "name-601");
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
        MemoryPayRunScheduleStoreFindsWhatItSaved();
    }
}
