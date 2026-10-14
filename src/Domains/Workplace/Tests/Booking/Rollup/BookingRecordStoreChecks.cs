namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class BookingRecordStoreChecks
{
    public static void MemoryBookingRecordStoreFindsWhatItSaved()
    {
        var store = BookingRecordStore.NewMemoryBookingRecordStore();
        var item = BookingRecord.NewBookingRecord("code-344", "label-759");
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
        MemoryBookingRecordStoreFindsWhatItSaved();
    }
}
