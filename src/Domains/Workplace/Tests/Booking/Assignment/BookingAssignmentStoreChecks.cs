namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class BookingAssignmentStoreChecks
{
    public static void MemoryBookingAssignmentStoreFindsWhatItSaved()
    {
        var store = BookingAssignmentStore.NewMemoryBookingAssignmentStore();
        var item = BookingAssignment.NewBookingAssignment("code-359", "name-538");
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
        MemoryBookingAssignmentStoreFindsWhatItSaved();
    }
}
