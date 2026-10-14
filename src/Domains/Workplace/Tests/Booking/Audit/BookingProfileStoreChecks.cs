namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingProfileStoreChecks
{
    public static void MemoryBookingProfileStoreFindsWhatItSaved()
    {
        var store = BookingProfileStore.NewMemoryBookingProfileStore();
        var item = BookingProfile.NewBookingProfile("code-249", "label-779");
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
        MemoryBookingProfileStoreFindsWhatItSaved();
    }
}
