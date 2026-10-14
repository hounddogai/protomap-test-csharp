namespace Acme.Workforce.Absence.Tests.Holiday.Schedule;

using Acme.Workforce.Absence.Holiday.Schedule;

/// <summary>Checks of schedule code, run by the project's runner.</summary>
public static class ManualHolidayVarianceStoreChecks
{
    public static void MemoryManualHolidayVarianceStoreFindsWhatItSaved()
    {
        var store = ManualHolidayVarianceStore.NewMemoryManualHolidayVarianceStore();
        var item = ManualHolidayVariance.NewManualHolidayVariance("code-884", "name-510");
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
        MemoryManualHolidayVarianceStoreFindsWhatItSaved();
    }
}
