namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class GlobalBookingTierStoreChecks
{
    public static void MemoryGlobalBookingTierStoreFindsWhatItSaved()
    {
        var store = GlobalBookingTierStore.NewMemoryGlobalBookingTierStore();
        var item = GlobalBookingTier.NewGlobalBookingTier("code-106", "title-494");
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
        MemoryGlobalBookingTierStoreFindsWhatItSaved();
    }
}
