namespace Acme.Workforce.Workplace.Tests.Booking.Digest;

using Acme.Workforce.Workplace.Booking.Digest;

/// <summary>Checks of digest code, run by the project's runner.</summary>
public static class CappedBookingPreferenceStoreChecks
{
    public static void MemoryCappedBookingPreferenceStoreFindsWhatItSaved()
    {
        var store = CappedBookingPreferenceStore.NewMemoryCappedBookingPreferenceStore();
        var item = CappedBookingPreference.NewCappedBookingPreference("code-162", "label-875");
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
        MemoryCappedBookingPreferenceStoreFindsWhatItSaved();
    }
}
