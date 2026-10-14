namespace Acme.Workforce.Workplace.Tests.Booking.BookingSegment;

using Acme.Workforce.Workplace.Booking.BookingSegment;

/// <summary>Checks of booking segment code, run by the project's runner.</summary>
public static class LegacyBookingVarianceStoreChecks
{
    public static void MemoryLegacyBookingVarianceStoreFindsWhatItSaved()
    {
        var store = LegacyBookingVarianceStore.NewMemoryLegacyBookingVarianceStore();
        var item = LegacyBookingVariance.NewLegacyBookingVariance("code-157", "name-577");
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
        MemoryLegacyBookingVarianceStoreFindsWhatItSaved();
    }
}
