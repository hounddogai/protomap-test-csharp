namespace Acme.Workforce.Assets.Tests.Laptop.Adjustment;

using Acme.Workforce.Assets.Laptop.Adjustment;

/// <summary>Checks of adjustment code, run by the project's runner.</summary>
public static class PendingLaptopReservationStoreChecks
{
    public static void MemoryPendingLaptopReservationStoreFindsWhatItSaved()
    {
        var store = PendingLaptopReservationStore.NewMemoryPendingLaptopReservationStore();
        var item = PendingLaptopReservation.NewPendingLaptopReservation("code-199", "title-197");
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
        MemoryPendingLaptopReservationStoreFindsWhatItSaved();
    }
}
