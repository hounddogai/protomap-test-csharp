namespace Acme.Workforce.Absence.Tests.Accrual.Schedule;

using Acme.Workforce.Absence.Accrual.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class PrimaryAccrualScheduleStoreChecks
{
    public static void MemoryPrimaryAccrualScheduleStoreFindsWhatItSaved()
    {
        var store = PrimaryAccrualScheduleStore.NewMemoryPrimaryAccrualScheduleStore();
        var item = PrimaryAccrualSchedule.NewPrimaryAccrualSchedule("code-131", "label-204");
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
        MemoryPrimaryAccrualScheduleStoreFindsWhatItSaved();
    }
}
