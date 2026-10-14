namespace Acme.Workforce.Workplace.Tests.Booking.BookingVariance;

using Acme.Workforce.Workplace.Booking.BookingVariance;

/// <summary>Checks of booking variance code, run by the project's runner.</summary>
public static class BookingDraftStoreChecks
{
    public static void MemoryBookingDraftStoreFindsWhatItSaved()
    {
        var store = BookingDraftStore.NewMemoryBookingDraftStore();
        var item = BookingDraft.NewBookingDraft("code-80", "title-795");
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
        MemoryBookingDraftStoreFindsWhatItSaved();
    }
}
