namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingPreferenceStoreChecks
{
    public static void MemoryManualBookingPreferenceStoreFindsWhatItSaved()
    {
        var store = ManualBookingPreferenceStore.NewMemoryManualBookingPreferenceStore();
        var item = ManualBookingPreference.NewManualBookingPreference("code-572", "title-166");
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
        MemoryManualBookingPreferenceStoreFindsWhatItSaved();
    }
}
