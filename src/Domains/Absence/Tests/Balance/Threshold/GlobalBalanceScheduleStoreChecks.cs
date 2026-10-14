namespace Acme.Workforce.Absence.Tests.Balance.Threshold;

using Acme.Workforce.Absence.Balance.Threshold;

/// <summary>Checks of threshold code, run by the project's runner.</summary>
public static class GlobalBalanceScheduleStoreChecks
{
    public static void MemoryGlobalBalanceScheduleStoreFindsWhatItSaved()
    {
        var store = GlobalBalanceScheduleStore.NewMemoryGlobalBalanceScheduleStore();
        var item = GlobalBalanceSchedule.NewGlobalBalanceSchedule("code-76", "title-282");
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
        MemoryGlobalBalanceScheduleStoreFindsWhatItSaved();
    }
}
