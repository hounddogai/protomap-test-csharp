namespace Acme.Workforce.Workplace.Tests.Booking.BookingAudit;

using Acme.Workforce.Workplace.Booking.BookingAudit;

/// <summary>Checks of booking audit code, run by the project's runner.</summary>
public static class BookingBatchStoreChecks
{
    public static void MemoryBookingBatchStoreFindsWhatItSaved()
    {
        var store = BookingBatchStore.NewMemoryBookingBatchStore();
        var item = BookingBatch.NewBookingBatch("code-95", "label-169");
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
        MemoryBookingBatchStoreFindsWhatItSaved();
    }
}
