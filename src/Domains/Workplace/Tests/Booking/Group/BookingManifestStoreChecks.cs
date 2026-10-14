namespace Acme.Workforce.Workplace.Tests.Booking.Group;

using Acme.Workforce.Workplace.Booking.Group;

/// <summary>Checks of group code, run by the project's runner.</summary>
public static class BookingManifestStoreChecks
{
    public static void MemoryBookingManifestStoreFindsWhatItSaved()
    {
        var store = BookingManifestStore.NewMemoryBookingManifestStore();
        var item = BookingManifest.NewBookingManifest("code-154", "name-5");
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
        MemoryBookingManifestStoreFindsWhatItSaved();
    }
}
