namespace Acme.Workforce.Workplace.Tests.Booking.BookingReservation;

using Acme.Workforce.Workplace.Booking.BookingReservation;

/// <summary>Checks of booking reservation code, run by the project's runner.</summary>
public static class CappedBookingThresholdStoreChecks
{
    public static void MemoryCappedBookingThresholdStoreFindsWhatItSaved()
    {
        var store = CappedBookingThresholdStore.NewMemoryCappedBookingThresholdStore();
        var item = CappedBookingThreshold.NewCappedBookingThreshold("code-643", "name-632");
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
        MemoryCappedBookingThresholdStoreFindsWhatItSaved();
    }
}
