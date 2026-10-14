namespace Acme.Workforce.Workplace.Tests.Booking.BookingReview;

using Acme.Workforce.Workplace.Booking.BookingReview;

/// <summary>Checks of booking review code, run by the project's runner.</summary>
public static class ManualBookingAssignmentStoreChecks
{
    public static void MemoryManualBookingAssignmentStoreFindsWhatItSaved()
    {
        var store = ManualBookingAssignmentStore.NewMemoryManualBookingAssignmentStore();
        var item = ManualBookingAssignment.NewManualBookingAssignment("code-835", "name-592");
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
        MemoryManualBookingAssignmentStoreFindsWhatItSaved();
    }
}
