namespace Acme.Workforce.Assets.Tests.Laptop.Review;

using Acme.Workforce.Assets.Laptop.Review;

/// <summary>Checks of review code, run by the project's runner.</summary>
public static class StrictLaptopReservationStoreChecks
{
    public static void MemoryStrictLaptopReservationStoreFindsWhatItSaved()
    {
        var store = StrictLaptopReservationStore.NewMemoryStrictLaptopReservationStore();
        var item = StrictLaptopReservation.NewStrictLaptopReservation("code-866", "title-127");
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
        MemoryStrictLaptopReservationStoreFindsWhatItSaved();
    }
}
