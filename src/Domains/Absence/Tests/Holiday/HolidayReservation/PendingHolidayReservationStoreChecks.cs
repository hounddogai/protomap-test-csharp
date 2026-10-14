namespace Acme.Workforce.Absence.Tests.Holiday.HolidayReservation;

using Acme.Workforce.Absence.Holiday.HolidayReservation;

/// <summary>Checks of holiday reservation code, run by the project's runner.</summary>
public static class PendingHolidayReservationStoreChecks
{
    public static void MemoryPendingHolidayReservationStoreFindsWhatItSaved()
    {
        var store = PendingHolidayReservationStore.NewMemoryPendingHolidayReservationStore();
        var item = PendingHolidayReservation.NewPendingHolidayReservation("code-689", "title-152");
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
        MemoryPendingHolidayReservationStoreFindsWhatItSaved();
    }
}
