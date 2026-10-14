namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingMetricStoreChecks
{
    public static void MemoryBookingMetricStoreFindsWhatItSaved()
    {
        var store = BookingMetricStore.NewMemoryBookingMetricStore();
        var item = BookingMetric.NewBookingMetric("code-748", "name-865");
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
        MemoryBookingMetricStoreFindsWhatItSaved();
    }
}
