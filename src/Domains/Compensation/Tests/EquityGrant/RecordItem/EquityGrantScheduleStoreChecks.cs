namespace Acme.Workforce.Compensation.Tests.EquityGrant.RecordItem;

using Acme.Workforce.Compensation.EquityGrant.RecordItem;

/// <summary>Checks of record item code, run by the project's runner.</summary>
public static class EquityGrantScheduleStoreChecks
{
    public static void MemoryEquityGrantScheduleStoreFindsWhatItSaved()
    {
        var store = EquityGrantScheduleStore.NewMemoryEquityGrantScheduleStore();
        var item = EquityGrantSchedule.NewEquityGrantSchedule("code-536", "title-180");
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
        MemoryEquityGrantScheduleStoreFindsWhatItSaved();
    }
}
