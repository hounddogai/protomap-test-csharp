namespace Acme.Workforce.Workplace.Tests.Occupancy.Reservation;

using Acme.Workforce.Workplace.Occupancy.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class OccupancyReservationStoreChecks
{
    public static void MemoryOccupancyReservationStoreFindsWhatItSaved()
    {
        var store = OccupancyReservationStore.NewMemoryOccupancyReservationStore();
        var item = OccupancyReservation.NewOccupancyReservation("code-934", "title-972");
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
        MemoryOccupancyReservationStoreFindsWhatItSaved();
    }
}
