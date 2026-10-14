namespace Acme.Workforce.Workplace.Tests.Booking.Plan;

using Acme.Workforce.Workplace.Booking.Plan;

/// <summary>Checks of plan code, run by the project's runner.</summary>
public static class BookingPlanStoreChecks
{
    public static void MemoryBookingPlanStoreFindsWhatItSaved()
    {
        var store = BookingPlanStore.NewMemoryBookingPlanStore();
        var item = BookingPlan.NewBookingPlan("code-618", "name-584");
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
        MemoryBookingPlanStoreFindsWhatItSaved();
    }
}
