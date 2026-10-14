namespace Acme.Workforce.Absence.Tests.Holiday.Reservation;

using Acme.Workforce.Absence.Holiday.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class HolidayReservationStoreChecks
{
    public static void MemoryHolidayReservationStoreFindsWhatItSaved()
    {
        var store = HolidayReservationStore.NewMemoryHolidayReservationStore();
        var item = HolidayReservation.NewHolidayReservation("code-33", "name-220");
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
        MemoryHolidayReservationStoreFindsWhatItSaved();
    }
}
