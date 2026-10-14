namespace Acme.Workforce.Assets.Tests.Laptop.Forecast;

using Acme.Workforce.Assets.Laptop.Forecast;

/// <summary>Checks of forecast code, run by the project's runner.</summary>
public static class LaptopReservationStoreChecks
{
    public static void MemoryLaptopReservationStoreFindsWhatItSaved()
    {
        var store = LaptopReservationStore.NewMemoryLaptopReservationStore();
        var item = LaptopReservation.NewLaptopReservation("code-671", "label-661");
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
        MemoryLaptopReservationStoreFindsWhatItSaved();
    }
}
