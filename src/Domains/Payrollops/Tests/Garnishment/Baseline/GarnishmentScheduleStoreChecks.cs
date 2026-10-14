namespace Acme.Workforce.Payrollops.Tests.Garnishment.Baseline;

using Acme.Workforce.Payrollops.Garnishment.Baseline;

/// <summary>Checks of baseline code, run by the project's runner.</summary>
public static class GarnishmentScheduleStoreChecks
{
    public static void MemoryGarnishmentScheduleStoreFindsWhatItSaved()
    {
        var store = GarnishmentScheduleStore.NewMemoryGarnishmentScheduleStore();
        var item = GarnishmentSchedule.NewGarnishmentSchedule("code-546", "title-269");
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
        MemoryGarnishmentScheduleStoreFindsWhatItSaved();
    }
}
