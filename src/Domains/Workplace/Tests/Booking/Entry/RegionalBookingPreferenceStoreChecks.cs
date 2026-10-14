namespace Acme.Workforce.Workplace.Tests.Booking.Entry;

using Acme.Workforce.Workplace.Booking.Entry;

/// <summary>Checks of entry code, run by the project's runner.</summary>
public static class RegionalBookingPreferenceStoreChecks
{
    public static void MemoryRegionalBookingPreferenceStoreFindsWhatItSaved()
    {
        var store = RegionalBookingPreferenceStore.NewMemoryRegionalBookingPreferenceStore();
        var item = RegionalBookingPreference.NewRegionalBookingPreference("code-562", "label-881");
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
        MemoryRegionalBookingPreferenceStoreFindsWhatItSaved();
    }
}
