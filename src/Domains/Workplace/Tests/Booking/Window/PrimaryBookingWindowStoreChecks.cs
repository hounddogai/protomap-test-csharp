namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class PrimaryBookingWindowStoreChecks
{
    public static void MemoryPrimaryBookingWindowStoreFindsWhatItSaved()
    {
        var store = PrimaryBookingWindowStore.NewMemoryPrimaryBookingWindowStore();
        var item = PrimaryBookingWindow.NewPrimaryBookingWindow("code-997", "name-788");
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
        MemoryPrimaryBookingWindowStoreFindsWhatItSaved();
    }
}
