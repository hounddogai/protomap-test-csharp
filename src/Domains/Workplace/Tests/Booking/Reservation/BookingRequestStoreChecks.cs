namespace Acme.Workforce.Workplace.Tests.Booking.Reservation;

using Acme.Workforce.Workplace.Booking.Reservation;

/// <summary>Checks of reservation code, run by the project's runner.</summary>
public static class BookingRequestStoreChecks
{
    public static void MemoryBookingRequestStoreFindsWhatItSaved()
    {
        var store = BookingRequestStore.NewMemoryBookingRequestStore();
        var item = BookingRequest.NewBookingRequest("code-336", "title-371");
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
        MemoryBookingRequestStoreFindsWhatItSaved();
    }
}
