namespace Acme.Workforce.Workplace.Tests.Booking.Audit;

using Acme.Workforce.Workplace.Booking.Audit;

/// <summary>Checks of audit code, run by the project's runner.</summary>
public static class BookingPreferenceStoreChecks
{
    public static void MemoryBookingPreferenceStoreFindsWhatItSaved()
    {
        var store = BookingPreferenceStore.NewMemoryBookingPreferenceStore();
        var item = BookingPreference.NewBookingPreference("code-246", "label-286");
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
        MemoryBookingPreferenceStoreFindsWhatItSaved();
    }
}
