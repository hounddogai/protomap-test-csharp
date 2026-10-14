namespace Acme.Workforce.Workplace.Tests.Booking.Window;

using Acme.Workforce.Workplace.Booking.Window;

/// <summary>Checks of window code, run by the project's runner.</summary>
public static class BookingBudgetStoreChecks
{
    public static void MemoryBookingBudgetStoreFindsWhatItSaved()
    {
        var store = BookingBudgetStore.NewMemoryBookingBudgetStore();
        var item = BookingBudget.NewBookingBudget("code-305", "label-878");
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
        MemoryBookingBudgetStoreFindsWhatItSaved();
    }
}
