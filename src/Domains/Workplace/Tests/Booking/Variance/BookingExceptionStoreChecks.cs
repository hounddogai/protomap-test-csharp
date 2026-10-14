namespace Acme.Workforce.Workplace.Tests.Booking.Variance;

using Acme.Workforce.Workplace.Booking.Variance;

/// <summary>Checks of variance code, run by the project's runner.</summary>
public static class BookingExceptionStoreChecks
{
    public static void MemoryBookingExceptionStoreFindsWhatItSaved()
    {
        var store = BookingExceptionStore.NewMemoryBookingExceptionStore();
        var item = BookingException.NewBookingException("code-763", "label-866");
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
        MemoryBookingExceptionStoreFindsWhatItSaved();
    }
}
