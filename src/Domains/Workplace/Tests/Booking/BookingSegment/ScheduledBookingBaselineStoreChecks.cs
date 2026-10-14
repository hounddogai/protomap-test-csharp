namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class ScheduledBookingBaselineStoreChecks
{
    public static void MemoryScheduledBookingBaselineStoreFindsWhatItSaved()
    {
        var store = ScheduledBookingBaselineStore.NewMemoryScheduledBookingBaselineStore();
        var item = ScheduledBookingBaseline.NewScheduledBookingBaseline("code-852", "title-869");
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
        MemoryScheduledBookingBaselineStoreFindsWhatItSaved();
    }
}
