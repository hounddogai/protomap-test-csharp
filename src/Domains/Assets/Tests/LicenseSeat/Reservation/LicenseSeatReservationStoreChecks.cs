namespace Acme.Workforce.Assets.Tests.LicenseSeat.Reservation;

using Acme.Workforce.Assets.LicenseSeat.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class LicenseSeatReservationStoreChecks
{
    public static void MemoryLicenseSeatReservationStoreFindsWhatItSaved()
    {
        var store = LicenseSeatReservationStore.NewMemoryLicenseSeatReservationStore();
        var item = LicenseSeatReservation.NewLicenseSeatReservation("code-999", "title-43");
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
        MemoryLicenseSeatReservationStoreFindsWhatItSaved();
    }
}
