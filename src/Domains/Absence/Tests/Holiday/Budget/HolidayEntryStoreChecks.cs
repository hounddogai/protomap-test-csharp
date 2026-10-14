namespace Acme.Workforce.Absence.Tests.Holiday.Budget;

using Acme.Workforce.Absence.Holiday.Budget;

/// <summary>Checks of budget code, run by the project's runner.</summary>
public static class HolidayEntryStoreChecks
{
    public static void MemoryHolidayEntryStoreFindsWhatItSaved()
    {
        var store = HolidayEntryStore.NewMemoryHolidayEntryStore();
        var item = HolidayEntry.NewHolidayEntry("code-966", "title-532");
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
        MemoryHolidayEntryStoreFindsWhatItSaved();
    }
}
