namespace Acme.Workforce.Orgdesign.Tests.Headcount.Tier;

using Acme.Workforce.Orgdesign.Headcount.Tier;

/// <summary>Checks of tier code, run by the project's runner.</summary>
public static class HeadcountScheduleStoreChecks
{
    public static void MemoryHeadcountScheduleStoreFindsWhatItSaved()
    {
        var store = HeadcountScheduleStore.NewMemoryHeadcountScheduleStore();
        var item = HeadcountSchedule.NewHeadcountSchedule("code-745", "label-489");
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
        MemoryHeadcountScheduleStoreFindsWhatItSaved();
    }
}
