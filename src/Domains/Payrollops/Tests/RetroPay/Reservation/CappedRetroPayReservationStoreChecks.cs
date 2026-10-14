namespace Acme.Workforce.Payrollops.Tests.RetroPay.Reservation;

using Acme.Workforce.Payrollops.RetroPay.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class CappedRetroPayReservationStoreChecks
{
    public static void MemoryCappedRetroPayReservationStoreFindsWhatItSaved()
    {
        var store = CappedRetroPayReservationStore.NewMemoryCappedRetroPayReservationStore();
        var item = CappedRetroPayReservation.NewCappedRetroPayReservation("code-992", "name-369");
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
        MemoryCappedRetroPayReservationStoreFindsWhatItSaved();
    }
}
