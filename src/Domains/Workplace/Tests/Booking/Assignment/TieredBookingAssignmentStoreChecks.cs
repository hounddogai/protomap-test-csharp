namespace Acme.Workforce.Workplace.Tests.Booking.Assignment;

using Acme.Workforce.Workplace.Booking.Assignment;

/// <summary>Checks of assignment code, run by the project's runner.</summary>
public static class TieredBookingAssignmentStoreChecks
{
    public static void MemoryTieredBookingAssignmentStoreFindsWhatItSaved()
    {
        var store = TieredBookingAssignmentStore.NewMemoryTieredBookingAssignmentStore();
        var item = TieredBookingAssignment.NewTieredBookingAssignment("code-308", "label-908");
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
        MemoryTieredBookingAssignmentStoreFindsWhatItSaved();
    }
}
