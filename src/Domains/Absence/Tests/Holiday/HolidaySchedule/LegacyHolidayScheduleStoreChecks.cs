namespace Acme.Workforce.Absence.Tests.Holiday.HolidaySchedule;

using Acme.Workforce.Absence.Holiday.HolidaySchedule;

/// <summary>Checks of holiday schedule code, run by the project's runner.</summary>
public static class LegacyHolidayScheduleStoreChecks
{
    public static void MemoryLegacyHolidayScheduleStoreFindsWhatItSaved()
    {
        var store = LegacyHolidayScheduleStore.NewMemoryLegacyHolidayScheduleStore();
        var item = LegacyHolidaySchedule.NewLegacyHolidaySchedule("code-283", "label-748");
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
        MemoryLegacyHolidayScheduleStoreFindsWhatItSaved();
    }
}
