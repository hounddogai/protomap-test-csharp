namespace Acme.Workforce.Workplace.Tests.Booking.StrictBookingPlan;

using Acme.Workforce.Workplace.Booking.StrictBookingPlan;

/// <summary>Checks of strict booking plan code, run by the project's runner.</summary>
public static class PendingBookingPlanStoreChecks
{
    public static void MemoryPendingBookingPlanStoreFindsWhatItSaved()
    {
        var store = PendingBookingPlanStore.NewMemoryPendingBookingPlanStore();
        var item = PendingBookingPlan.NewPendingBookingPlan("code-998", "label-164");
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
        MemoryPendingBookingPlanStoreFindsWhatItSaved();
    }
}
