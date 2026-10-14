namespace Acme.Workforce.Absence.Tests.Holiday.Request;

using Acme.Workforce.Absence.Holiday.Request;

/// <summary>Checks of request code, run by the project's runner.</summary>
public static class HolidayRequestStoreChecks
{
    public static void MemoryHolidayRequestStoreFindsWhatItSaved()
    {
        var store = HolidayRequestStore.NewMemoryHolidayRequestStore();
        var item = HolidayRequest.NewHolidayRequest("code-964", "name-882");
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
        MemoryHolidayRequestStoreFindsWhatItSaved();
    }
}
