namespace Acme.Workforce.Absence.Tests.Holiday.OverrideItem;

using Acme.Workforce.Absence.Holiday.OverrideItem;

/// <summary>Checks of override item code, run by the project's runner.</summary>
public static class HolidayExceptionStoreChecks
{
    public static void MemoryHolidayExceptionStoreFindsWhatItSaved()
    {
        var store = HolidayExceptionStore.NewMemoryHolidayExceptionStore();
        var item = HolidayException.NewHolidayException("code-672", "title-760");
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
        MemoryHolidayExceptionStoreFindsWhatItSaved();
    }
}
