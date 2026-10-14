namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class TieredBookingRecordStoreChecks
{
    public static void MemoryTieredBookingRecordStoreFindsWhatItSaved()
    {
        var store = TieredBookingRecordStore.NewMemoryTieredBookingRecordStore();
        var item = TieredBookingRecord.NewTieredBookingRecord("code-399", "name-393");
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
        MemoryTieredBookingRecordStoreFindsWhatItSaved();
    }
}
