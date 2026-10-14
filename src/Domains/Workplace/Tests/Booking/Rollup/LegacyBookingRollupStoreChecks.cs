namespace Acme.Workforce.Workplace.Tests.Booking.Rollup;

using Acme.Workforce.Workplace.Booking.Rollup;

/// <summary>Checks of rollup code, run by the project's runner.</summary>
public static class LegacyBookingRollupStoreChecks
{
    public static void MemoryLegacyBookingRollupStoreFindsWhatItSaved()
    {
        var store = LegacyBookingRollupStore.NewMemoryLegacyBookingRollupStore();
        var item = LegacyBookingRollup.NewLegacyBookingRollup("code-960", "title-565");
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
        MemoryLegacyBookingRollupStoreFindsWhatItSaved();
    }
}
