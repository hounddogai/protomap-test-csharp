namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class SeasonalHolidayExceptionStoreChecks
{
    public static void MemorySeasonalHolidayExceptionStoreFindsWhatItSaved()
    {
        var store = SeasonalHolidayExceptionStore.NewMemorySeasonalHolidayExceptionStore();
        var item = SeasonalHolidayException.NewSeasonalHolidayException("code-528", "name-753");
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
        MemorySeasonalHolidayExceptionStoreFindsWhatItSaved();
    }
}
